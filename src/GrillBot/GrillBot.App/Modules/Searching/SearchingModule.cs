using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Services;
using GrillBot.Data;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Searching
{
    [Group("hledam")]
    [Name("Hledání (něčeho, třeba týmu)")]
    [RequireContext(ContextType.Guild, ErrorMessage = "Tento příkaz lze provést pouze na serveru.")]
    public class SearchingModule : Infrastructure.ModuleBase
    {
        private SearchingService Service { get; }

        public SearchingModule(SearchingService service)
        {
            Service = service;
        }

        [Command("")]
        [Summary("Vytvoří hledání.")]
        public async Task CreateSearchAsync([Remainder][Name("obsah")] string _)
        {
            await Service.CreateAsync(Context.Guild, Context.User, Context.Channel, Context.Message);
            await Context.Message.AddReactionAsync(Emojis.Ok);
        }

        [Command("remove")]
        [Summary("Smaže hledání.")]
        public async Task RemoveSearchAsync(long id)
        {
            var isAdmin = Context.User is IGuildUser guildUser && (guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageMessages);

            try
            {
                await Service.RemoveSearchAsync(id, Context.User, isAdmin);
                await Context.Message.AddReactionAsync(Emojis.Ok);
            }
            catch (UnauthorizedAccessException ex)
            {
                await Context.Message.AddReactionAsync(Emojis.Nok);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("")]
        [Summary("Vypíše hledání v daném kanálu.")]
        public async Task GetSearchingsAsync([Name("kanal")] ISocketMessageChannel channel = null)
        {
            if (channel == null) channel = Context.Channel;

            var data = await Service.GetSearchListAsync(Context.Guild, channel, 0);

            if (data.Count == 0)
            {
                await ReplyAsync($"V kanálu {channel.GetMention()} zatím nikdo nic nehledá.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithSearching(data, channel, Context.Guild, 0, Context.User);

            var message = await ReplyAsync(embed: embed.Build());
            await message.AddReactionsAsync(new[] { Emojis.MoveToNext, Emojis.MoveToPrev });
        }
    }
}
