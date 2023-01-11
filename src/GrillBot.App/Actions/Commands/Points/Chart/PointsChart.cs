using GrillBot.App.Infrastructure.IO;
using GrillBot.App.Services.Graphics;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Models.API.Points;
using GrillBot.Database.Models;
using GrillBot.Database.Services.Repository;
using ImageMagick;

namespace GrillBot.App.Actions.Commands.Points.Chart;

public class PointsChart : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private IGraphicsClient GraphicsClient { get; }
    private RandomizationManager RandomizationManager { get; }

    // Dictionary<UserId, List<(Day, MessagePoints, ReactionPoints)>
    private Dictionary<ulong, List<(DateTime day, int messagePoints, int reactionPoints)>>? UsersData { get; set; }
    private List<(DateTime day, int messagePoints, int reactionPoints)>? GuildData { get; set; }

    public PointsChart(GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, IGraphicsClient graphicsClient, RandomizationManager randomizationManager)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        GraphicsClient = graphicsClient;
        RandomizationManager = randomizationManager;
    }

    public async Task<TemporaryFile> ProcessAsync(ChartType type, IEnumerable<IUser>? users, ChartsFilter filter)
    {
        await PrepareDataAsync(type, users, filter);

        var charts = new List<MagickImage>();
        var resultFile = new TemporaryFile("png");

        try
        {
            await RenderChartsAsync(charts, filter, type);
            await MergeAndSaveCharts(resultFile, charts);
        }
        finally
        {
            charts.ForEach(o => o.Dispose());
        }

        return resultFile;
    }

    private async Task PrepareDataAsync(ChartType type, IEnumerable<IUser>? users, ChartsFilter filter)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        switch (type)
        {
            case ChartType.GuildChart:
                await PrepareGuildDataAsync(repository, filter);
                break;
            case ChartType.UserChart:
                UsersData = new Dictionary<ulong, List<(DateTime day, int messagePoints, int reactionPoints)>>();
                if (users != null)
                {
                    foreach (var user in users)
                        await PrepareUserDataAsync(repository, user, filter);
                }

                if (!UsersData.ContainsKey(Context.User.Id))
                    await PrepareUserDataAsync(repository, Context.User, filter);
                break;
        }
    }

    private async Task PrepareUserDataAsync(GrillBotRepository repository, IUser user, ChartsFilter filter)
    {
        var parameters = CreateParameters(filter);
        parameters.UserId = user.Id.ToString();

        UsersData!.Add(user.Id, await repository.Points.GetGraphDataAsync(parameters));
    }

    private async Task PrepareGuildDataAsync(GrillBotRepository repository, ChartsFilter filter)
    {
        var parameters = CreateParameters(filter);
        GuildData = await repository.Points.GetGraphDataAsync(parameters);
    }

    private GetPointTransactionsParams CreateParameters(ChartsFilter filter)
    {
        return new GetPointTransactionsParams
        {
            Merged = false,
            Sort = { Descending = false, OrderBy = "AssignedAt" },
            AssignedAt = new RangeParams<DateTime?>
            {
                From = DateTime.Now.AddYears(-1)
            },
            GuildId = Context.Guild.Id.ToString(),
            OnlyMessages = (filter & ChartsFilter.Messages) != 0 && (filter & ChartsFilter.Reactions) == 0 && (filter & ChartsFilter.Summary) == 0,
            OnlyReactions = (filter & ChartsFilter.Reactions) != 0 && (filter & ChartsFilter.Messages) == 0 && (filter & ChartsFilter.Summary) == 0
        };
    }

    private async Task RenderChartsAsync(ICollection<MagickImage> result, ChartsFilter filter, ChartType type)
    {
        if (type == ChartType.GuildChart)
        {
            var renderer = new GuildChartRenderer(Texts, GraphicsClient, Locale);

            if ((filter & ChartsFilter.Messages) != ChartsFilter.None)
                result.Add(await renderer.RenderAsync(Context.Guild, GuildData!, ChartsFilter.Messages));
            if ((filter & ChartsFilter.Reactions) != ChartsFilter.None)
                result.Add(await renderer.RenderAsync(Context.Guild, GuildData!, ChartsFilter.Reactions));
            if ((filter & ChartsFilter.Summary) != ChartsFilter.None)
                result.Add(await renderer.RenderAsync(Context.Guild, GuildData!, ChartsFilter.Summary));
        }
        else if (type == ChartType.UserChart)
        {
            var renderer = new UsersChartRenderer(Texts, GraphicsClient, RandomizationManager, Locale);

            if ((filter & ChartsFilter.Messages) != ChartsFilter.None)
                result.Add(await renderer.RenderAsync(Context.Guild, UsersData!, ChartsFilter.Messages));
            if ((filter & ChartsFilter.Reactions) != ChartsFilter.None)
                result.Add(await renderer.RenderAsync(Context.Guild, UsersData!, ChartsFilter.Reactions));
            if ((filter & ChartsFilter.Summary) != ChartsFilter.None)
                result.Add(await renderer.RenderAsync(Context.Guild, UsersData!, ChartsFilter.Summary));
        }
    }

    private static async Task MergeAndSaveCharts(TemporaryFile resultFile, IReadOnlyList<MagickImage> charts)
    {
        var finalWidth = charts.Count * ChartRequestBuilder.Height;
        using var image = new MagickImage(new MagickColor(ChartRequestBuilder.Background), ChartRequestBuilder.Width, finalWidth);

        IDrawables<byte> drawables = new Drawables();
        for (var i = 0; i < charts.Count; i++)
        {
            var top = i * ChartRequestBuilder.Height;
            drawables = drawables.Composite(0, top, CompositeOperator.Multiply, charts[i]);

            if (i > 0)
                drawables = drawables.Line(0, top, ChartRequestBuilder.Width, top);
        }

        drawables.Draw(image);
        await image.WriteAsync(resultFile.Path);
    }
}
