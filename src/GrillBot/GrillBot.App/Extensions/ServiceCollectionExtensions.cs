using GrillBot.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace GrillBot.App.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddTriggeredJob<T>(this IServiceCollectionQuartzConfigurator quartz, IConfiguration configuration, string configurationKey, bool useCron = false) where T : IJob
    {
        var jobName = typeof(T).Name;

        quartz.AddJob<T>(opt => opt.WithIdentity(jobName))
            .AddTrigger(opt =>
            {
                opt
                    .ForJob(jobName)
                    .WithIdentity($"{jobName}-Trigger");

                if (useCron)
                    opt.WithCronSchedule(configuration.GetValue<string>(configurationKey));
                else
                    opt.WithSimpleSchedule(builder => builder.RepeatForever().WithInterval(configuration.GetValue<TimeSpan>(configurationKey)));
            });
    }
    
    public static IHttpClientBuilder AddHttpClient(this IServiceCollection services, IConfiguration configuration, string serviceId, string serviceConfigName)
    {
        return services.AddHttpClient(serviceId, client =>
        {
            client.BaseAddress = new Uri(configuration[$"Services:{serviceConfigName}:Api"]);
            client.Timeout = TimeSpan.FromMilliseconds(configuration[$"Services:{serviceConfigName}:Timeout"].ToInt());
        });
    }
}
