using Discord;
using Discord.WebSocket;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services;
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
            if (!TryGetEmbedAndMetadata<SearchingMetadata>(message, emote, out var embed, out var metadata)) return false;

            var guild = DiscordClient.GetGuild(metadata.GuildId);
            if (guild == null) return false;

            var channel = guild.GetTextChannel(metadata.ChannelId);
            if (channel == null) return false;

            int newPage = GetPageNumber(metadata.Page, int.MaxValue, emote);
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
