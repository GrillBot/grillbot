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
using GrillBot.App.Managers.DataResolve;

namespace GrillBot.App.Actions.Commands.Points;

public class PointsLeaderboard : CommandAction
{
    private const int MaxItemsCount = 10;

    private ITextsManager Texts { get; }
    private FormatHelper FormatHelper { get; }
    private IPointsServiceClient PointsServiceClient { get; }
    private readonly DataResolveManager _dataResolveManager;

    public PointsLeaderboard(ITextsManager texts, FormatHelper formatHelper, IPointsServiceClient pointsServiceClient,
        DataResolveManager dataResolveManager)
    {
        Texts = texts;
        FormatHelper = formatHelper;
        PointsServiceClient = pointsServiceClient;
        _dataResolveManager = dataResolveManager;
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
            var username = await ResolveUsernameAsync(item.UserId.ToUlong(), guild);

            result.Add(FormatRow(i, item, skip, username, overAllTime));
        }

        return result;
    }

    private async Task<string> ResolveUsernameAsync(ulong userId, IGuild guild)
    {
        var guildUser = await _dataResolveManager.GetGuildUserAsync(guild.Id, userId);
        if (guildUser is null)
            return $"UnknownUser {userId}";

        var entity = new Database.Entity.GuildUser
        {
            Nickname = guildUser.Nickname,
            User = new Database.Entity.User
            {
                Username = guildUser.Username,
                GlobalAlias = guildUser.GlobalAlias
            }
        };

        return entity.DisplayName!;
    }

    private string FormatRow(int index, PointsStatus item, int skip, string username, bool overAllTime)
    {
        var value = overAllTime ? item.Total : item.YearBack;
        var points = FormatHelper.FormatNumber("Points/Board/Counts", Locale, value);
        return Texts["Points/Board/Row", Locale].FormatWith(index + skip + 1, username, points);
    }

    private async Task<MessageComponent?> CreatePaginationComponents(int currentPage)
    {
        var pagesCount = await ComputePagesCountAsync();
        return ComponentsHelper.CreatePaginationComponents(currentPage, pagesCount, "points");
    }
}
