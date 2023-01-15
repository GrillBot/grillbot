using GrillBot.Common.Extensions;
using GrillBot.Common.Services.Graphics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Common.Services;

public static class ServicesExtensions
{
    public static IHttpClientBuilder AddHttpClient(this IServiceCollection services, IConfiguration configuration, string serviceId, string serviceConfigName)
    {
        return services.AddHttpClient(serviceId, client =>
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

        return services;
    }
}
