using Discord;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Services;
using GrillBot.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Searching
{
    public class SearchingReactionHandler : ReactionEventHandler
    {
        private SearchingService SearchingService { get; }
        private DiscordSocketClient DiscordClient { get; }

        public SearchingReactionHandler(SearchingService service, DiscordSocketClient discordClient)
        {
            SearchingService = service;
            DiscordClient = discordClient;
        }

        public override async Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user)
        {
            var embed = message.Embeds.FirstOrDefault();

            if (embed == null || embed.Footer == null || embed.Author == null) return false;
            if (!Emojis.PaginationEmojis.Any(o => o.IsEqual(emote))) return false;
            if (message.ReferencedMessage == null) return false;
            if (!embed.TryParseMetadata<SearchingMetadata>(out var metadata)) return false;

            var guild = DiscordClient.GetGuild(metadata.GuildId);
            if (guild == null) return false;

            var channel = guild.GetTextChannel(metadata.ChannelId);
            if (channel == null) return false;

            int newPage = metadata.Page;
            if (emote.IsEqual(Emojis.MoveToNext)) newPage++;
            else if (emote.IsEqual(Emojis.MoveToPrev)) newPage--;

            if (newPage < 0) newPage = 0;
            if (newPage == metadata.Page) return false;

            var data = await SearchingService.GetSearchListAsync(guild, channel, newPage);
            if (data.Count == 0) return false;

            var resultEmbed = new EmbedBuilder()
                .WithSearching(data, channel, guild, newPage, user);

            await message.ModifyAsync(msg => msg.Embed = resultEmbed.Build());
            await message.RemoveReactionAsync(emote, user);
            return true;
        }
    }
}
