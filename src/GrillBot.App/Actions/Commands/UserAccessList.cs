using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.User;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Actions.Commands;

public class UserAccessList : CommandAction
{
    private ITextsManager Texts { get; }

    public UserAccessList(ITextsManager texts)
    {
        Texts = texts;
    }

    public async Task<(Embed embed, MessageComponent? paginationComponent)> ProcessAsync(IGuildUser user, int page)
    {
        var visibleChannels = await Context.Guild.GetAvailableChannelsAsync(user);
        var embed = CreateEmbed(user, page);
        var fields = CreateFields(visibleChannels);
        SetPage(embed, fields, page, out var pagesCount);

        var paginationComponents = ComponentsHelper.CreatePaginationComponents(page, pagesCount, "user_access");
        return (embed.Build(), paginationComponents);
    }

    private EmbedBuilder CreateEmbed(IGuildUser user, int page)
    {
        return new EmbedBuilder()
            .WithFooter(Context.User)
            .WithAuthor(Texts["User/AccessList/Title", Locale].FormatWith(user.GetFullName()), user.GetUserAvatarUrl())
            .WithMetadata(new UserAccessListMetadata { ForUserId = user.Id, Page = page })
            .WithColor(user.GetHighestRole(true)?.Color ?? Color.Blue)
            .WithCurrentTimestamp();
    }

    private void SetPage(EmbedBuilder builder, IReadOnlyList<EmbedFieldBuilder> fields, int page, out int pagesCount)
    {
        if (fields.Count == 0)
        {
            builder.WithDescription(Texts["User/AccessList/NoAccess", Locale]);
            pagesCount = 1;
            return;
        }

        var pages = SplitToPages(fields, builder);
        pagesCount = pages.Count;
        builder.WithFields(pages[page]);
    }

    private List<EmbedFieldBuilder> CreateFields(IEnumerable<IGuildChannel> visibleChannels)
    {
        var visibleChannelsQuery = visibleChannels.Where(o => o is not IThreadChannel && o is not ICategoryChannel);
        var categorizedChannels = SplitToCategories(visibleChannelsQuery);
        var result = new List<EmbedFieldBuilder>();

        var fieldBuilder = new StringBuilder();
        foreach (var category in categorizedChannels)
        {
            foreach (var channel in category.Value.OrderBy(o => o.Name).Select(o => o.GetHyperlink()))
            {
                if (fieldBuilder.Length + channel.Length + 1 >= EmbedFieldBuilder.MaxFieldValueLength)
                {
                    result.Add(new EmbedFieldBuilder().WithName(category.Key).WithValue(fieldBuilder.ToString().Trim()));
                    fieldBuilder.Clear();
                }

                fieldBuilder.Append(channel).Append(", ");
            }

            if (fieldBuilder.Length <= 0)
                continue;

            result.Add(new EmbedFieldBuilder().WithName(category.Key).WithValue(fieldBuilder.ToString()));
            fieldBuilder.Clear();
        }

        foreach (var field in result)
        {
            var fieldValue = field.Value.ToString()!.Trim();
            if (fieldValue.EndsWith(','))
                fieldValue = fieldValue[..^1];
            field.WithValue(fieldValue);
        }

        return result;
    }

    private IEnumerable<KeyValuePair<string, List<IGuildChannel>>> SplitToCategories(IEnumerable<IGuildChannel> channels)
    {
        var result = new Dictionary<string, List<IGuildChannel>>();
        var withoutCategory = Texts["User/AccessList/WithoutCategory", Locale];

        foreach (var channel in channels)
        {
            var category = channel.GetCategory();
            var categoryName = category?.Name ?? withoutCategory;

            if (!result.ContainsKey(categoryName))
                result.Add(categoryName, new List<IGuildChannel>());
            result[categoryName].Add(channel);
        }

        return result
            .OrderBy(o => o.Key == withoutCategory ? " " : o.Key);
    }

    public async Task<int> ComputePagesCount(IGuildUser user)
    {
        var visibleChannels = await Context.Guild.GetAvailableChannelsAsync(user);
        var fields = CreateFields(visibleChannels);
        var embed = CreateEmbed(user, 0);
        return SplitToPages(fields, embed).Count;
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
