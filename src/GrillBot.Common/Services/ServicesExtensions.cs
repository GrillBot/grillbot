using GrillBot.Common.Services.AuditLog;
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
    private static void AddHttpClient(this IServiceCollection services, IConfiguration configuration, string serviceId)
    {
        services.AddHttpClient(serviceId, client =>
        {
            client.BaseAddress = new Uri(configuration[$"Services:{serviceId}:Api"]!);
            client.Timeout = TimeSpan.FromMilliseconds(configuration[$"Services:{serviceId}:Timeout"]!.ToInt());
        });
    }

    private static void AddService<TInterface, TImplementation>(this IServiceCollection services, IConfiguration configuration, string serviceName)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services
            .AddScoped<TInterface, TImplementation>()
            .AddHttpClient(configuration, serviceName);
    }

    public static void AddThirdPartyServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddService<IGraphicsClient, GraphicsClient>(configuration, "Graphics");
        services.AddService<IKachnaOnlineClient, KachnaOnlineClient>(configuration, "KachnaOnline");
        services.AddService<IMathClient, MathClient>(configuration, "Math");
        services.AddService<IRubbergodServiceClient, RubbergodServiceClient>(configuration, "RubbergodService");
        services.AddService<IFileServiceClient, FileServiceClient>(configuration, "FileService");
        services.AddService<IPointsServiceClient, PointsServiceClient>(configuration, "PointsService");
        services.AddService<IImageProcessingClient, ImageProcessingClient>(configuration, "ImageProcessing");
        services.AddService<IAuditLogServiceClient, AuditLogServiceClient>(configuration, "AuditLog");
    }
}
