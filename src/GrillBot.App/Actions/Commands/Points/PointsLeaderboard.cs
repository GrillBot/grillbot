using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Points;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Exceptions;
using GrillBot.Database.Models.Points;

namespace GrillBot.App.Actions.Commands.Points;

public class PointsLeaderboard : CommandAction
{
    private const int MaxItemsCount = 10;

    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private FormatHelper FormatHelper { get; }

    public PointsLeaderboard(GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, FormatHelper formatHelper)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        FormatHelper = formatHelper;
    }

    public async Task<(Embed embed, MessageComponent paginationComponent)> ProcessAsync(int page)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var skip = page * MaxItemsCount;
        var guildIds = new[] { Context.Guild.Id.ToString() };
        var data = await repository.Points.GetPointsBoardDataAsync(guildIds, MaxItemsCount, skip: skip, allColumns: false);
        if (data.Count == 0)
            throw new NotFoundException(Texts["Points/Board/NoActivity", Locale]);

        var embed = CreateEmbed(data, page, skip);
        var paginationComponents = await CreatePaginationComponents(page);
        return (embed, paginationComponents);
    }

    public async Task<int> ComputePagesCountAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildIds = new[] { Context.Guild.Id.ToString() };
        var totalCount = await repository.Points.GetPointsBoardCountAsync(guildIds);
        return (int)Math.Ceiling(totalCount / (double)MaxItemsCount);
    }

    private Embed CreateEmbed(IEnumerable<PointBoardItem> points, int page, int skip)
    {
        var list = string.Join("\n", points.Select((o, i) => FormatRow(i, o, skip)));

        return new EmbedBuilder()
            .WithFooter(Context.User)
            .WithMetadata(new PointsBoardMetadata { Page = page })
            .WithAuthor(Texts["Points/Board/Title", Locale])
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithDescription(list)
            .Build();
    }

    private string FormatRow(int index, PointBoardItem item, int skip)
    {
        var points = FormatHelper.FormatNumber("Points/Board/Counts", Locale, item.PointsYearBack);
        return Texts["Points/Board/Row", Locale].FormatWith(index + skip + 1, item.GuildUser.FullName(), points);
    }

    private async Task<MessageComponent> CreatePaginationComponents(int currentPage)
    {
        var pagesCount = await ComputePagesCountAsync();
        return ComponentsHelper.CreatePaginationComponents(currentPage, pagesCount, "points");
    }
}
