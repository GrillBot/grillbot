using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Data.Models;

namespace GrillBot.App.Modules.Implementations.Emotes;

public static class EmoteListExtensions
{
    public static EmbedBuilder WithEmoteList(this EmbedBuilder embed, List<EmoteStatItem> data, IUser user, IUser ofUser, IGuild guild,
        string sortQuery, int page = 0)
    {
        embed.WithFooter(user);
        embed.WithMetadata(new EmoteListMetadata() { Page = page, GuildId = guild.Id, SortQuery = sortQuery, OfUserId = ofUser?.Id });

        embed.WithAuthor("Statistika použivání emotů");
        embed.WithColor(Color.Blue);
        embed.WithCurrentTimestamp();

        if (data.Count == 0)
        {
            if (ofUser != null)
                embed.WithDescription($"Pro uživatele `{ofUser.GetDisplayName()}` ještě nebyla zaznamenáno žádné použití emotu.");
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
