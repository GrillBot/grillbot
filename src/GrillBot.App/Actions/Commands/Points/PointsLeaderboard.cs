using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Points;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Commands.Points;

public class PointsLeaderboard : CommandAction
{
    private const int MaxItemsCount = 10;

    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private FormatHelper FormatHelper { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    public PointsLeaderboard(GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, FormatHelper formatHelper, IPointsServiceClient pointsServiceClient)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        FormatHelper = formatHelper;
        PointsServiceClient = pointsServiceClient;
    }

    public async Task<(Embed embed, MessageComponent? paginationComponent)> ProcessAsync(int page)
    {
        var skip = page * MaxItemsCount;
        var guildId = Context.Guild.Id.ToString();

        var leaderboard = await PointsServiceClient.GetLeaderboardAsync(guildId, skip, MaxItemsCount, true);
        leaderboard.ValidationErrors.AggregateAndThrow();

        if (leaderboard.Response!.Count == 0)
            throw new NotFoundException(Texts["Points/Board/NoActivity", Locale]);

        await using var repository = DatabaseBuilder.CreateRepository();

        var embed = await CreateEmbedAsync(repository, Context.Guild, leaderboard.Response, page, skip);
        var paginationComponents = await CreatePaginationComponents(page);
        return (embed, paginationComponents);
    }

    public async Task<int> ComputePagesCountAsync()
    {
        var totalItemsCount = await PointsServiceClient.GetLeaderboardCountAsync(Context.Guild.Id.ToString());
        return ComputePagesCount(totalItemsCount);
    }

    private static int ComputePagesCount(long totalItemsCount)
        => (int)Math.Ceiling(totalItemsCount / (double)MaxItemsCount);

    private async Task<Embed> CreateEmbedAsync(GrillBotRepository repository, IGuild guild, IReadOnlyList<BoardItem> points, int page, int skip)
    {
        var list = string.Join("\n", await FormatRowsAsync(repository, points, skip, guild));

        return new EmbedBuilder()
            .WithFooter(Context.User)
            .WithMetadata(new PointsBoardMetadata { Page = page })
            .WithAuthor(Texts["Points/Board/Title", Locale])
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithDescription(list)
            .Build();
    }

    private async Task<List<string>> FormatRowsAsync(GrillBotRepository repository, IReadOnlyList<BoardItem> board, int skip, IGuild guild)
    {
        var result = new List<string>();

        for (var i = 0; i < board.Count; i++)
        {
            var item = board[i];
            var user = await guild.GetUserAsync(item.UserId.ToUlong());
            if (user is not null)
                result.Add(FormatRow(i, item, skip, user));
        }

        return result;
    }

    private string FormatRow(int index, PointsStatus item, int skip, IGuildUser guildUser)
    {
        var points = FormatHelper.FormatNumber("Points/Board/Counts", Locale, item.YearBack);
        return Texts["Points/Board/Row", Locale].FormatWith(index + skip + 1, guildUser.GetFullName(), points);
    }

    private async Task<MessageComponent?> CreatePaginationComponents(int currentPage)
    {
        var pagesCount = await ComputePagesCountAsync();
        return ComponentsHelper.CreatePaginationComponents(currentPage, pagesCount, "points");
    }
}
