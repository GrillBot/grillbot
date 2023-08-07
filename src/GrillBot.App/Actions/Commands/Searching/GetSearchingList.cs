using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Searching;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Searching;

namespace GrillBot.App.Actions.Commands.Searching;

public class GetSearchingList : CommandAction
{
    private Api.V1.Searching.GetSearchingList GetSearchingListAction { get; }
    private ITextsManager Texts { get; }

    public GetSearchingList(Api.V1.Searching.GetSearchingList getSearchingListAction, ITextsManager texts)
    {
        GetSearchingListAction = getSearchingListAction;
        Texts = texts;
    }

    public async Task<(Embed embed, MessageComponent? paginationComponent)> ProcessAsync(int page, string? query, IChannel? channel)
    {
        GetSearchingListAction.UpdateContext(Locale, Context.User);
        channel ??= Context.Channel;

        var embed = CreateEmptyEmbed(page, query, channel);
        var parameters = CreateParameters(query, channel);
        var list = await GetSearchingListAction.ProcessAsync(parameters);
        var fields = CreateFields(list);
        SetPage(embed, fields, page, channel, query, out var pagesCount);

        var paginationComponents = ComponentsHelper.CreatePaginationComponents(page, pagesCount, "search");
        return (embed.Build(), paginationComponents);
    }

    public async Task<int> ComputePagesCountAsync(string? query, IChannel channel)
    {
        GetSearchingListAction.UpdateContext(Locale, Context.User);

        var parameters = CreateParameters(query, channel);
        var list = await GetSearchingListAction.ProcessAsync(parameters);
        var fields = CreateFields(list);
        var embed = CreateEmptyEmbed(0, query, channel);
        return EmbedHelper.SplitToPages(fields, embed).Count;
    }

    private GetSearchingListParams CreateParameters(string? query, IChannel channel)
    {
        return new GetSearchingListParams
        {
            Pagination = { Page = 0, PageSize = int.MaxValue },
            Sort = { Descending = false, OrderBy = "Id" },
            ChannelId = channel.Id.ToString(),
            GuildId = Context.Guild.Id.ToString(),
            MessageQuery = query
        };
    }

    private EmbedBuilder CreateEmptyEmbed(int page, string? query, IChannel channel)
    {
        return new EmbedBuilder()
            .WithFooter(Context.User)
            .WithMetadata(new SearchingMetadata { Page = page, MessageQuery = query, ChannelId = channel.Id })
            .WithTitle(Texts["SearchingModule/List/Embed/Title", Locale].FormatWith(channel.Name))
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();
    }

    private static List<EmbedFieldBuilder> CreateFields(PaginatedResponse<SearchingListItem> list)
    {
        if (list.TotalItemsCount == 0)
            return new List<EmbedFieldBuilder>();

        return list.Data
            .ConvertAll(o => new EmbedFieldBuilder().WithName($"**{o.Id} - **{o.User.Username}**").WithValue(FixMessage(o.Message)));
    }

    private static string FixMessage(string message)
    {
        if (!message.StartsWith('"')) message = "\"" + message;
        if (!message.EndsWith('"')) message += "\"";

        return message;
    }

    private void SetPage(EmbedBuilder embed, IReadOnlyList<EmbedFieldBuilder> fields, int page, IChannel channel, string? query, out int pagesCount)
    {
        if (fields.Count == 0)
        {
            var textsKey = string.IsNullOrEmpty(query) ? "NoItems" : "NoItemsWithQuery";
            embed.WithDescription(Texts[$"SearchingModule/List/Embed/{textsKey}", Locale].FormatWith(channel.GetHyperlink(Context.Guild)));
            pagesCount = 1;
            return;
        }

        var pages = EmbedHelper.SplitToPages(fields, embed);
        pagesCount = pages.Count;
        embed.WithFields(pages[page]);
    }
}
