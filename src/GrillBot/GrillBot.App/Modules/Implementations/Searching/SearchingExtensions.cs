using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Models;

namespace GrillBot.App.Modules.Implementations.Searching;

public static class SearchingExtensions
{
    public static EmbedBuilder WithSearching(this EmbedBuilder embed, PaginatedResponse<SearchingListItem> items, ITextChannel channel, IGuild guild, int page, IUser user,
        string messageQuery)
    {
        embed.WithFooter(user);
        embed.WithMetadata(new SearchingMetadata { ChannelId = channel.Id, GuildId = guild.Id, Page = page, MessageQuery = messageQuery });

        embed.WithAuthor($"Hledání v kanálu #{channel.Name}");
        embed.WithColor(Color.Blue);
        embed.WithCurrentTimestamp();

        if (items.TotalItemsCount == 0)
        {
            embed.WithDescription(
                $"V kanálu {channel.GetMention()} zatím nikdo nic nehledá." +
                (!string.IsNullOrEmpty(messageQuery) ? " Zkus jiný vyhledávací podřetězec." : "")
            );
        }
        else
        {
            items.Data.ForEach(o => embed.AddField(
                $"**{o.Id}** - **{o.User.Username}**",
                FixMessage(o.Message)
            ));
        }

        return embed;
    }

    private static string FixMessage(string message)
    {
        if (!message.StartsWith("\"")) message = "\"" + message;
        if (!message.EndsWith("\"")) message += "\"";

        return message;
    }
}
