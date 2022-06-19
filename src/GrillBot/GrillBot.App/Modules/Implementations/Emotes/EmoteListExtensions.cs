using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Modules.Implementations.Emotes;

public static class EmoteListExtensions
{
    public static EmbedBuilder WithEmoteList(this EmbedBuilder embed, List<EmoteStatItem> data, IUser user, IUser ofUser, IGuild guild,
        string orderBy, bool descending, int page = 0)
    {
        embed.WithFooter(user);
        embed.WithMetadata(new EmoteListMetadata { Page = page, GuildId = guild.Id, OfUserId = ofUser?.Id, OrderBy = orderBy, Descending = descending });

        embed.WithAuthor("Statistika použivání emotů");
        embed.WithColor(Color.Blue);
        embed.WithCurrentTimestamp();

        if (data.Count == 0)
        {
            embed.WithDescription(
                ofUser != null ? $"Pro uživatele `{ofUser.GetDisplayName()}` ještě nebyla zaznamenáno žádné použití emotu." : "Ještě nebylo zaznamenáno žádné použití emotu."
            );
        }
        else
        {
            foreach (var item in data)
            {
                var formatted = string.Join("\n",
                    $"Počet použití: **{item.UseCount}**",
                    $"Použilo uživatelů: **{item.UsedUsersCount}**",
                    $"První použití: **{item.FirstOccurence.ToCzechFormat()}**",
                    $"Poslední použití: **{item.LastOccurence.ToCzechFormat()}**"
                );

                embed.AddField(item.Emote.FullId, formatted, true);
            }
        }

        return embed;
    }
}
