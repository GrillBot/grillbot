using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Models;

namespace GrillBot.App.Modules.Implementations.Emotes;

public static class EmoteListExtensions
{
    public static EmbedBuilder WithEmoteList(this EmbedBuilder embed, List<EmoteStatItem> data, IUser user, IUser forUser,
        bool isPrivate, bool desc, string sortBy, int page = 0)
    {
        embed.WithFooter(user);
        embed.WithMetadata(new EmoteListMetadata() { Page = page, IsPrivate = isPrivate, SortBy = sortBy, Desc = desc, OfUserId = forUser?.Id });

        embed.WithAuthor("Statistika použivání emotů");
        embed.WithColor(Color.Blue);
        embed.WithCurrentTimestamp();

        if (data.Count == 0)
        {
            if (forUser != null)
                embed.WithDescription($"Pro uživatele `{forUser.GetDisplayName()}` ještě nebyla zaznamenáno žádné použití emotu.");
            else
                embed.WithDescription("Ještě nebylo zaznamenáno žádné použití emotu.");
        }
        else
        {
            data.ForEach(o => embed.AddField(o.Id, o.ToString(), true));
        }

        return embed;
    }
}
