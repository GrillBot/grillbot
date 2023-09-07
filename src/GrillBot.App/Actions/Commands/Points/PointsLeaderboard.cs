using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Points;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Core.Services.PointsService.Enums;

namespace GrillBot.App.Actions.Commands.Points;

public class PointsLeaderboard : CommandAction
{
    private const int MaxItemsCount = 10;

    private ITextsManager Texts { get; }
    private FormatHelper FormatHelper { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    public PointsLeaderboard(ITextsManager texts, FormatHelper formatHelper, IPointsServiceClient pointsServiceClient)
    {
        Texts = texts;
        FormatHelper = formatHelper;
        PointsServiceClient = pointsServiceClient;
    }

    public async Task<(Embed embed, MessageComponent? paginationComponent)> ProcessAsync(int page, bool overAllTime)
    {
        var skip = page * MaxItemsCount;
        var guildId = Context.Guild.Id.ToString();

        var leaderboard = await GetLeaderboardAsync(guildId, skip, overAllTime);
        if (leaderboard.Count == 0)
            throw new NotFoundException(Texts["Points/Board/NoActivity", Locale]);

        var embed = await CreateEmbedAsync(Context.Guild, leaderboard, page, skip, overAllTime);
        var paginationComponents = await CreatePaginationComponents(page);
        return (embed, paginationComponents);
    }

    private async Task<List<BoardItem>> GetLeaderboardAsync(string guildId, int skip, bool overAllTime)
    {
        var columns = overAllTime ? LeaderboardColumnFlag.Total : LeaderboardColumnFlag.YearBack;
        var sortOptions = overAllTime ? LeaderboardSortOptions.ByTotalDescending : LeaderboardSortOptions.ByYearBackDescending;

        var leaderboard = await PointsServiceClient.GetLeaderboardAsync(guildId, skip, MaxItemsCount, columns, sortOptions);
        leaderboard.ValidationErrors.AggregateAndThrow();

        return leaderboard.Response!;
    }

    public async Task<int> ComputePagesCountAsync()
    {
        var totalItemsCount = await PointsServiceClient.GetLeaderboardCountAsync(Context.Guild.Id.ToString());
        return ComputePagesCount(totalItemsCount);
    }

    private static int ComputePagesCount(long totalItemsCount)
        => (int)Math.Ceiling(totalItemsCount / (double)MaxItemsCount);

    private async Task<Embed> CreateEmbedAsync(IGuild guild, IReadOnlyList<BoardItem> points, int page, int skip, bool overAllTime)
    {
        var list = string.Join("\n", await FormatRowsAsync(points, skip, guild, overAllTime));

        return new EmbedBuilder()
            .WithFooter(Context.User)
            .WithMetadata(new PointsBoardMetadata
            {
                Page = page,
                OverAllTime = overAllTime
            })
            .WithAuthor(Texts["Points/Board/Title", Locale])
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithDescription(list)
            .Build();
    }

    private async Task<List<string>> FormatRowsAsync(IReadOnlyList<BoardItem> board, int skip, IGuild guild, bool overAllTime)
    {
        var result = new List<string>();

        for (var i = 0; i < board.Count; i++)
        {
            var item = board[i];
            var user = await guild.GetUserAsync(item.UserId.ToUlong());
            if (user is not null)
                result.Add(FormatRow(i, item, skip, user, overAllTime));
        }

        return result;
    }

    private string FormatRow(int index, PointsStatus item, int skip, IGuildUser guildUser, bool overAllTime)
    {
        var value = overAllTime ? item.Total : item.YearBack;
        var points = FormatHelper.FormatNumber("Points/Board/Counts", Locale, value);
        return Texts["Points/Board/Row", Locale].FormatWith(index + skip + 1, guildUser.GetFullName(), points);
    }

    private async Task<MessageComponent?> CreatePaginationComponents(int currentPage)
    {
        var pagesCount = await ComputePagesCountAsync();
        return ComponentsHelper.CreatePaginationComponents(currentPage, pagesCount, "points");
    }
}
