using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Searching;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Models;

namespace GrillBot.App.Actions.Commands.Searching;

public class GetSearchingList : CommandAction
{
    private Api.V1.Searching.GetSearchingList GetSearchingListAction { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public GetSearchingList(Api.V1.Searching.GetSearchingList getSearchingListAction, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts)
    {
        GetSearchingListAction = getSearchingListAction;
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task<(Embed embed, MessageComponent paginationComponent)> ProcessAsync(int page, string query, IChannel channel)
    {
        GetSearchingListAction.UpdateContext(Context.Interaction.UserLocale, Context.User);
        channel ??= Context.Channel;

        var parameters = CreateParameters(page, query, channel);
        var list = await GetSearchingListAction.ProcessAsync(parameters);
        var pagesCount = ComputePagesCountAsync(list.TotalItemsCount);
        var embed = CreateEmbed(list, page, query, channel);
        var paginationComponents = ComponentsHelper.CreatePaginationComponents(page, pagesCount, "search");

        return (embed, paginationComponents);
    }

    public async Task<int> ComputePagesCountAsync(string query, IChannel channel)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var parameters = CreateParameters(0, query, channel);
        var totalCount = await repository.Searching.GetSearchesCountAsync(parameters, new List<string>());
        return ComputePagesCountAsync(totalCount);
    }

    private static int ComputePagesCountAsync(long totalCount)
        => (int)Math.Ceiling(totalCount / (double)EmbedBuilder.MaxFieldCount);

    private GetSearchingListParams CreateParameters(int page, string query, IChannel channel)
    {
        return new GetSearchingListParams
        {
            Pagination = { Page = page, PageSize = EmbedBuilder.MaxFieldCount },
            Sort = { Descending = false, OrderBy = "Id" },
            ChannelId = channel.Id.ToString(),
            GuildId = Context.Guild.Id.ToString(),
            MessageQuery = query
        };
    }

    private Embed CreateEmbed(PaginatedResponse<SearchingListItem> list, int page, string query, IChannel channel)
    {
        var embed = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithMetadata(new SearchingMetadata { Page = page, MessageQuery = query, ChannelId = channel.Id })
            .WithTitle(Texts["SearchingModule/List/Embed/Title", Locale].FormatWith(channel.Name))
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();

        if (list.TotalItemsCount == 0)
        {
            var textsKey = string.IsNullOrEmpty(query) ? "NoItems" : "NoItemsWithQuery";
            embed.WithDescription(Texts[$"SearchingModule/List/Embed/{textsKey}", Locale].FormatWith(channel.GetMention()));
        }
        else
        {
            foreach (var item in list.Data)
                embed.AddField($"**{item.Id} - **{item.User.Username}**", FixMessage(item.Message));
        }

        return embed.Build();
    }

    private static string FixMessage(string message)
    {
        if (!message.StartsWith("\"")) message = "\"" + message;
        if (!message.EndsWith("\"")) message += "\"";

        return message;
    }
}
