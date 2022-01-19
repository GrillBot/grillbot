using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Data.Models;

namespace GrillBot.App.Modules.Implementations.Searching;

public static class SearchingExtensions
{
    public static EmbedBuilder WithSearching(this EmbedBuilder embed, List<SearchingItem> items, ISocketMessageChannel channel, IGuild guild, int page, IUser user)
    {
        embed.WithFooter(user);
        embed.WithMetadata(new SearchingMetadata() { ChannelId = channel.Id, GuildId = guild.Id, Page = page });

        embed.WithAuthor($"Hledání v kanálu #{channel.Name}");
        embed.WithColor(Color.Blue);
        embed.WithCurrentTimestamp();

        items.ForEach(o => embed.AddField(
            $"**{o.Id}** - **{o.DisplayName}**",
            $"\"{o.Message}\" [Jump]({o.JumpLink})"
        ));

        return embed;
    }
}
