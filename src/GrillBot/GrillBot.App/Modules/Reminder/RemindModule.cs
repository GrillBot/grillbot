using Discord;
using Discord.Commands;
using GrillBot.App.Extensions;
using GrillBot.App.Services.Reminder;
using GrillBot.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Reminder
{
    [Group("remind")]
    [Name("Připomínání")]
    [Infrastructure.Preconditions.RequireUserPermission(new[] { ChannelPermission.SendMessages }, false)]
    public class RemindModule : Infrastructure.ModuleBase
    {
        private RemindService RemindService { get; }

        public RemindModule(RemindService remindService)
        {
            RemindService = remindService;
        }

        [Command("")]
        [Summary("Vytvoří připomenutí k určitému datu. Připomenutí pro sebe lze klíčovým slovem `me`. Datum a čas musí být v budoucnosti a musí být později, než 5 minut od doby, založení připomenutí.")]
        [RequireBotPermission(ChannelPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám v tomto kanálu oprávnění přidávat reakce.")]
        public async Task CreateAsync([Name("komu")] IUser who, [Name("kdy")] DateTime at, [Remainder][Name("zprava")] string message)
        {
            var time = message.ParseTime();
            if (time != null)
            {
                at = at.Date.Add(time.Value);

                var parts = message.Split(' ');
                message = parts.Length == 1 ? null : string.Join(" ", parts[1..]);
            }

            try
            {
                await RemindService.CreateRemindAsync(Context.User, who, at, message, Context.Message);

                if (!Context.IsPrivate)
                {
                    await ReplyAsync($"Upozornění vytvořeno. Pokud si někdo přeje dostat toto upozornění také, tak ať dá na zprávu s příkazem reakci {Emojis.PersonRisingHand}");
                    await Context.Message.AddReactionAsync(Emojis.PersonRisingHand);
                }
                else
                {
                    await ReplyAsync("Upozornění vytvořeno.");
                }
            }
            catch (ValidationException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("")]
        [Summary("Získá seznam čekajících upozornění pro daného uživatele.")]
        [RequireBotPermission(ChannelPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám v tomto kanálu oprávnění přidávat reakce.")]
        public async Task GetRemindListAsync()
        {
            var data = await RemindService.GetRemindersAsync(Context.User, 0);

            var embed = await new EmbedBuilder()
                .WithRemindListAsync(data, Context.Client, Context.User, Context.User, 0);

            var message = await ReplyAsync(embed: embed.Build());
            if (data.Count >= EmbedBuilder.MaxFieldCount)
                await message.AddReactionsAsync(Emojis.PaginationEmojis);
        }

        [Command("cancel")]
        [Alias("zrusit")]
        [Summary("Zruší upozornění. Případně může upozornit předčasně.")]
        public async Task CancelReminderAsync(long id, [Name("upozornit")] bool notify = false)
        {
            try
            {
                await RemindService.CancelRemindAsync(id, Context.User, notify);
            }
            catch (ValidationException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }
    }
}
