using Discord;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure.Embeds;
using System;
using System.Collections.Generic;

namespace GrillBot.App.Modules.Emotes
{
    public static class EmoteListExtensions
    {
        public static EmbedBuilder WithEmoteList(this EmbedBuilder embed, List<Tuple<string, int, long, DateTime, DateTime>> data, IUser user, IUser forUser,
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
                data.ForEach(o => embed.AddField(o.Item1, FormatEmoteListItem(o.Item2, o.Item3, o.Item4, o.Item5), true));
            }

            return embed;
        }

        private static string FormatEmoteListItem(int usersCount, long useCount, DateTime firstOccurence, DateTime lastOccurence)
        {
            return string.Join("\n", new[]
            {
                $"Počet použití: **{useCount}**",
                $"Použilo uživatelů: **{usersCount}**",
                $"První použití: **{firstOccurence.ToCzechFormat()}**",
                $"Poslední použití: **{lastOccurence.ToCzechFormat()}**"
            });
        }
    }
}
