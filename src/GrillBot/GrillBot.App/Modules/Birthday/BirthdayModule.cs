using Discord;
using Discord.Commands;
using GrillBot.App.Services.Birthday;
using GrillBot.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Birthday
{
    [Group("birthday")]
    [Alias("narozeniny")]
    [Name("Narozeniny")]
    [Infrastructure.Preconditions.RequireUserPermission(new[] { ChannelPermission.SendMessages }, false)]
    public class BirthdayModule : Infrastructure.ModuleBase
    {
        private BirthdayService BirthdayService { get; }
        private IConfiguration Configuration { get; }

        public BirthdayModule(BirthdayService birthdayService, IConfiguration configuration)
        {
            BirthdayService = birthdayService;
            Configuration = configuration;
        }

        [Command("")]
        [Summary("Přidání data narození a včasné upozornění.\n" +
            "Pokud si nepřejete ukládat věk, tak jako rok dejte rok (přesně) `0001`.\n" +
            "Celý příkaz pak vypadá např.:\n`{prefix}narozeniny 1998-04-01` nebo `{prefix}narozeniny 0001-04-01`")]
        [RequireBotPermission(ChannelPermission.AddReactions, ErrorMessage = "Nelze provést tento příkaz, protože nemám práva přidávat reakce.")]
        public async Task AddAsync([Remainder][Name("kdy")] DateTime when)
        {
            await BirthdayService.AddBirthdayAsync(Context.User, when);
            await Context.Message.AddReactionAsync(Emojis.Ok);

            if (!Context.IsPrivate && Context.Guild.CurrentUser.GuildPermissions.ManageMessages)
                await Context.Message.DeleteAsync();
        }

        [Command("have?")]
        [Alias("mam?")]
        [Summary("Zjištění, zda uživatel má uložení narozeniny.")]
        public async Task HaveAsync()
        {
            if (await BirthdayService.HaveBirthdayAsync(Context.User))
                await ReplyAsync("Ano. Máš uložené narozeniny.");
            else
                await ReplyAsync("Ne. Nemáš uložené narozeniny.");
        }

        [Command("remove")]
        [Alias("gone", "pryc", "pryč", "smazat")]
        [Summary("Smazání data narození a ukončení upozornění.")]
        [RequireBotPermission(ChannelPermission.AddReactions, ErrorMessage = "Nelze provést tento příkaz, protože nemám práva přidávat reakce.")]
        public async Task RemoveAsync()
        {
            await BirthdayService.RemoveBirthdayAsync(Context.User);
            await Context.Message.AddReactionAsync(Emojis.Ok);
        }

        [Command("")]
        [Summary("Vypíše, kdo má dnes narozeniny.")]
        public async Task TodayBrithdayAsync()
        {
            var users = await BirthdayService.GetTodayBirthdaysAsync();
            await ReplyAsync(BirthdayHelper.Format(users, Configuration));
        }
    }
}
