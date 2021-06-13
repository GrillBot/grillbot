using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services.CronJobs
{
    static public class CronJobsExtensions
    {
        static public IServiceCollection AddCronJob<TCron>(this IServiceCollection services) where TCron : CronJobTask
        {
            services.AddHostedService<TCron>();
            return services;
        }
    }
}
