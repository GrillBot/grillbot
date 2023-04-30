using GrillBot.Common.Services.FileService;
using GrillBot.Common.Services.Graphics;
using GrillBot.Common.Services.ImageProcessing;
using GrillBot.Common.Services.KachnaOnline;
using GrillBot.Common.Services.Math;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.RubbergodService;
using GrillBot.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Common.Services;

public static class ServicesExtensions
{
    private static void AddHttpClient(this IServiceCollection services, IConfiguration configuration, string serviceId, string serviceConfigName)
    {
        services.AddHttpClient(serviceId, client =>
        {
            client.BaseAddress = new Uri(configuration[$"Services:{serviceConfigName}:Api"]!);
            client.Timeout = TimeSpan.FromMilliseconds(configuration[$"Services:{serviceConfigName}:Timeout"]!.ToInt());
        });
    }

    public static IServiceCollection AddThirdPartyServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddScoped<IGraphicsClient, GraphicsClient>()
            .AddHttpClient(configuration, "Graphics", "Graphics");

        services
            .AddScoped<IKachnaOnlineClient, KachnaOnlineClient>()
            .AddHttpClient(configuration, "KachnaOnline", "KachnaOnline");

        services
            .AddScoped<IMathClient, MathClient>()
            .AddHttpClient(configuration, "Math", "Math");

        services
            .AddScoped<IRubbergodServiceClient, RubbergodServiceClient>()
            .AddHttpClient(configuration, "RubbergodService", "RubbergodService");

        services
            .AddScoped<IFileServiceClient, FileServiceClient>()
            .AddHttpClient(configuration, "FileService", "FileService");

        services
            .AddScoped<IPointsServiceClient, PointsServiceClient>()
            .AddHttpClient(configuration, "PointsService", "PointsService");

        services
            .AddScoped<IImageProcessingClient, ImageProcessingClient>()
            .AddHttpClient(configuration, "ImageProcessing", "ImageProcessing");

        return services;
    }
}
