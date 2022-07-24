using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Modules.Implementations.User;

public static class UserAccessListExtensions
{
    public static EmbedBuilder WithUserAccessList(this EmbedBuilder embed, List<IGuildChannel> data, IUser forUser, IUser user, IGuild guild, int page,
        out int pagesCount)
    {
        embed.WithFooter(user);
        embed.WithAuthor($"Seznam přístupů pro uživatele {forUser.GetFullName()}", forUser.GetUserAvatarUrl());
        embed.WithMetadata(new UserAccessListMetadata { ForUserId = forUser.Id, GuildId = guild.Id, Page = page });

        embed.WithColor(Color.Blue);
        embed.WithCurrentTimestamp();

        if (data.Count == 0)
        {
            embed.WithDescription("Uživatel nemá přístup do žádného kanálu.");
            pagesCount = 1;
        }
        else
        {
            var fields = CreateFields(data);
            var pages = SplitToPages(fields, embed);

            pagesCount = pages.Count;
            embed.WithFields(pages[page]);
        }

        return embed;
    }

    private static List<EmbedFieldBuilder> CreateFields(List<IGuildChannel> channels)
    {
        const string separator = ", ";

        var categoryGroups = SplitToCategories(channels);

        var result = new List<EmbedFieldBuilder>();
        var fieldBuilder = new StringBuilder();

        foreach (var category in categoryGroups)
        {
            foreach (var channel in category.Value.OrderBy(o => o.Name).Select(o => o.GetMention()))
            {
                if (fieldBuilder.Length + channel.Length + separator.Length >= EmbedFieldBuilder.MaxFieldValueLength)
                {
                    result.Add(new EmbedFieldBuilder().WithName(category.Key).WithValue(fieldBuilder.ToString()));
                    fieldBuilder.Clear();
                    continue;
                }

                fieldBuilder.Append(channel).Append(separator);
            }

            if (fieldBuilder.Length <= 0)
                continue;

            result.Add(new EmbedFieldBuilder().WithName(category.Key).WithValue(fieldBuilder.ToString()));
            fieldBuilder.Clear();
        }

        return result;
    }

    private static Dictionary<string, List<IGuildChannel>> SplitToCategories(List<IGuildChannel> channels)
    {
        const string withoutCategory = "Bez kategorie";
        var result = new Dictionary<string, List<IGuildChannel>>();

        foreach (var channel in channels)
        {
            var category = channel.GetCategory();
            var categoryName = category?.Name ?? withoutCategory;

            if (!result.ContainsKey(categoryName))
                result.Add(categoryName, new List<IGuildChannel>());

            result[categoryName].Add(channel);
        }

        return result
            .OrderBy(o => o.Key == withoutCategory ? "" : o.Key)
            .ToDictionary(o => o.Key, o => o.Value);
    }

    private static List<List<EmbedFieldBuilder>> SplitToPages(IReadOnlyList<EmbedFieldBuilder> fields, EmbedBuilder embed)
    {
        var embedClone = embed.Build().ToEmbedBuilder();
        var pages = new List<List<EmbedFieldBuilder>>();

        for (var i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            embedClone.AddField(field);

            if (embedClone.Length > EmbedBuilder.MaxEmbedLength)
            {
                pages.Add(embedClone.Fields.Take(embedClone.Fields.Count - 1).ToList());
                embedClone.Fields.Clear();
                i--;
            }
            else if (embedClone.Fields.Count == EmbedBuilder.MaxFieldCount)
            {
                pages.Add(embedClone.Fields);
                embedClone.Fields.Clear();
            }
        }

        if (embedClone.Fields.Count > 0)
            pages.Add(embedClone.Fields);

        return pages;
    }
}
