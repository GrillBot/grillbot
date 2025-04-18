﻿using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.IO;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.ImageProcessing;
using GrillBot.Core.Services.ImageProcessing.Models;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Actions.Commands.Points.Chart;

public class PointsChart : CommandAction
{
    private IServiceProvider ServiceProvider { get; }
    private readonly IServiceClientExecutor<IImageProcessingClient> _imageProcessingClient;
    private readonly IServiceClientExecutor<IPointsServiceClient> _pointsServiceClient;

    private Dictionary<ulong, List<PointsChartItem>>? UsersData { get; set; }
    private List<PointsChartItem>? GuildData { get; set; }

    public PointsChart(IServiceProvider serviceProvider, IServiceClientExecutor<IPointsServiceClient> pointsServiceClient, IServiceClientExecutor<IImageProcessingClient> imageProcessingClient)
    {
        ServiceProvider = serviceProvider;
        _pointsServiceClient = pointsServiceClient;
        _imageProcessingClient = imageProcessingClient;
    }

    public async Task<TemporaryFile> ProcessAsync(ChartType type, IEnumerable<IUser>? users, ChartsFilter filter)
    {
        await PrepareDataAsync(type, users, filter);

        var request = await CreateRequestAsync(filter, type);
        var image = await _imageProcessingClient.ExecuteRequestAsync((c, ctx) => c.CreateChartImageAsync(request, ctx.CancellationToken));

        var resultFile = new TemporaryFile("png");
        await resultFile.WriteStreamAsync(image);
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
                UsersData = [];
                if (users != null)
                {
                    foreach (var user in users.Where(o => o.IsUser()).DistinctBy(o => o.Id))
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

        var data = await _pointsServiceClient.ExecuteRequestAsync((c, ctx) => c.GetChartDataAsync(request, ctx.CancellationToken));
        UsersData!.Add(user.Id, data);
    }

    private async Task PrepareGuildDataAsync(ChartsFilter filter)
    {
        var request = CreateParameters(filter);
        GuildData = await _pointsServiceClient.ExecuteRequestAsync((c, ctx) => c.GetChartDataAsync(request, ctx.CancellationToken));
    }

    private AdminListRequest CreateParameters(ChartsFilter filter)
    {
        return new AdminListRequest
        {
            ShowMerged = false,
            Sort = { Descending = false, OrderBy = "AssignedAt" },
            CreatedFrom = DateTime.UtcNow.AddYears(-1),
            CreatedTo = DateTime.MaxValue.ToUniversalTime(),
            GuildId = Context.Guild.Id.ToString(),
            OnlyMessages = (filter & ChartsFilter.Messages) != 0 && (filter & ChartsFilter.Reactions) == 0 && (filter & ChartsFilter.Summary) == 0,
            OnlyReactions = (filter & ChartsFilter.Reactions) != 0 && (filter & ChartsFilter.Messages) == 0 && (filter & ChartsFilter.Summary) == 0
        };
    }

    private async Task<ChartRequest> CreateRequestAsync(ChartsFilter filter, ChartType type)
    {
        switch (type)
        {
            case ChartType.GuildChart:
                {
                    var builder = ServiceProvider.GetRequiredService<GuildChartBuilder>().Init(GuildData!, Context.Guild);
                    return await builder.CreateRequestAsync(filter, Locale);
                }
            case ChartType.UserChart:
                {
                    var builder = ServiceProvider.GetRequiredService<UserChartBuilder>().Init(UsersData!, Context.Guild);
                    return await builder.CreateRequestAsync(filter, Locale);
                }
            default:
                throw new NotSupportedException();
        }
    }
}
