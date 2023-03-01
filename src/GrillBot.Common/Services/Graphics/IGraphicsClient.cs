﻿using GrillBot.Common.Services.Common;
using GrillBot.Common.Services.Graphics.Models.Chart;
using GrillBot.Common.Services.Graphics.Models.Diagnostics;
using GrillBot.Common.Services.Graphics.Models.Images;

namespace GrillBot.Common.Services.Graphics;

public interface IGraphicsClient : IClient
{
    Task<byte[]> CreateChartAsync(ChartRequestData request);
    Task<Metrics> GetMetricsAsync();
    Task<string> GetVersionAsync();
    Task<Stats> GetStatisticsAsync();
    Task<byte[]> CreateWithoutAccidentImage(WithoutAccidentRequestData request);
    Task<byte[]> CreatePointsImageAsync(PointsImageRequest imageRequest);
    Task<List<byte[]>> CreatePeepoAngryAsync(List<byte[]> avatarFrames);
    Task<List<byte[]>> CreatePeepoLoveAsync(List<byte[]> avatarFrames);
}
