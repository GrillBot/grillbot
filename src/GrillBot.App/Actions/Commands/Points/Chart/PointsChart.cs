using GrillBot.App.Infrastructure.IO;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.PointsService.Models;
using GrillBot.Core.Extensions;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Actions.Commands.Points.Chart;

public class PointsChart : CommandAction
{
    private IServiceProvider ServiceProvider { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    private Dictionary<ulong, List<PointsChartItem>>? UsersData { get; set; }
    private List<PointsChartItem>? GuildData { get; set; }

    public PointsChart(IServiceProvider serviceProvider, IPointsServiceClient pointsServiceClient)
    {
        ServiceProvider = serviceProvider;
        PointsServiceClient = pointsServiceClient;
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
        switch (type)
        {
            case ChartType.GuildChart:
                await PrepareGuildDataAsync(filter);
                break;
            case ChartType.UserChart:
                UsersData = new Dictionary<ulong, List<PointsChartItem>>();
                if (users != null)
                {
                    foreach (var user in users.Where(o => o.IsUser()))
                        await PrepareUserDataAsync(user, filter);
                }

                if (!UsersData.ContainsKey(Context.User.Id))
                    await PrepareUserDataAsync(Context.User, filter);
                break;
        }
    }

    private async Task PrepareUserDataAsync(IUser user, ChartsFilter filter)
    {
        var request = CreateParameters(filter);
        request.UserId = user.Id.ToString();

        var data = await PointsServiceClient.GetChartDataAsync(request);
        data.ValidationErrors.AggregateAndThrow();

        UsersData!.Add(user.Id, data.Response!);
    }

    private async Task PrepareGuildDataAsync(ChartsFilter filter)
    {
        var request = CreateParameters(filter);

        var data = await PointsServiceClient.GetChartDataAsync(request);
        data.ValidationErrors.AggregateAndThrow();

        GuildData = data.Response;
    }

    private AdminListRequest CreateParameters(ChartsFilter filter)
    {
        return new AdminListRequest
        {
            ShowMerged = false,
            Sort = { Descending = false, OrderBy = "AssignedAt" },
            CreatedFrom = DateTime.Now.AddYears(-1),
            GuildId = Context.Guild.Id.ToString(),
            OnlyMessages = (filter & ChartsFilter.Messages) != 0 && (filter & ChartsFilter.Reactions) == 0 && (filter & ChartsFilter.Summary) == 0,
            OnlyReactions = (filter & ChartsFilter.Reactions) != 0 && (filter & ChartsFilter.Messages) == 0 && (filter & ChartsFilter.Summary) == 0
        };
    }

    private async Task RenderChartsAsync(ICollection<MagickImage> result, ChartsFilter filter, ChartType type)
    {
        if (type == ChartType.GuildChart)
        {
            var renderer = ServiceProvider.GetRequiredService<GuildChartRenderer>();

            if ((filter & ChartsFilter.Messages) != ChartsFilter.None)
                result.Add(await renderer.RenderAsync(Context.Guild, GuildData!, ChartsFilter.Messages, Locale));
            if ((filter & ChartsFilter.Reactions) != ChartsFilter.None)
                result.Add(await renderer.RenderAsync(Context.Guild, GuildData!, ChartsFilter.Reactions, Locale));
            if ((filter & ChartsFilter.Summary) != ChartsFilter.None)
                result.Add(await renderer.RenderAsync(Context.Guild, GuildData!, ChartsFilter.Summary, Locale));
        }
        else if (type == ChartType.UserChart)
        {
            var renderer = ServiceProvider.GetRequiredService<UsersChartRenderer>();

            if ((filter & ChartsFilter.Messages) != ChartsFilter.None)
                result.Add(await renderer.RenderAsync(Context.Guild, UsersData!, ChartsFilter.Messages, Locale));
            if ((filter & ChartsFilter.Reactions) != ChartsFilter.None)
                result.Add(await renderer.RenderAsync(Context.Guild, UsersData!, ChartsFilter.Reactions, Locale));
            if ((filter & ChartsFilter.Summary) != ChartsFilter.None)
                result.Add(await renderer.RenderAsync(Context.Guild, UsersData!, ChartsFilter.Summary, Locale));
        }
    }

    private static async Task MergeAndSaveCharts(TemporaryFile resultFile, IReadOnlyList<MagickImage> charts)
    {
        var finalHeight = charts.Count * ChartRequestBuilder.Height;
        using var image = new MagickImage(new MagickColor(ChartRequestBuilder.Background), ChartRequestBuilder.Width, finalHeight);

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
