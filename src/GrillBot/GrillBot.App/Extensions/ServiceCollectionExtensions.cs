using Quartz;

namespace GrillBot.App.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTriggeredJob<T>(this IServiceCollectionQuartzConfigurator quartz, IConfiguration configuration, string configurationKey) where T : IJob
        {
            var jobName = typeof(T).Name;
            var period = configuration.GetValue<TimeSpan>(configurationKey);

            quartz.AddJob<T>(opt => opt.WithIdentity(jobName))
                .AddTrigger(opt => opt
                    .ForJob(jobName)
                    .WithIdentity($"{jobName}-Trigger")
                    .WithSimpleSchedule(builder => builder.RepeatForever().WithInterval(period))
                );
        }
    }
}
