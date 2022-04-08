using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.Logging;
using Quartz;

namespace GrillBot.App.Services.Birthday;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class BirthdayCronJob : Job
{
    private BirthdayService BirthdayService { get; }
    private IConfiguration Configuration { get; }

    public BirthdayCronJob(IConfiguration configuration, BirthdayService service, LoggingService logging,
        AuditLogService auditLogService, IDiscordClient discordClient, DiscordInitializationService initializationService)
        : base(logging, auditLogService, discordClient, initializationService)
    {
        BirthdayService = service;
        Configuration = configuration;
    }

    public override async Task RunAsync(IJobExecutionContext context)
    {
        var birthdays = await BirthdayService.GetTodayBirthdaysAsync(context.CancellationToken);
        if (birthdays.Count == 0)
        {
            context.Result = "NoBirthdays";
            return;
        }

        var birthdayNotificationSection = Configuration.GetSection("Birthday:Notifications");
        var guild = await DiscordClient.GetGuildAsync(birthdayNotificationSection.GetValue<ulong>("GuildId"));
        if (guild == null)
        {
            context.Result = "MissingGuild";
            return;
        }

        var channel = await guild.GetTextChannelAsync(birthdayNotificationSection.GetValue<ulong>("ChannelId"));
        if (channel == null)
        {
            context.Result = "MissingChannel";
            return;
        }

        var formatted = BirthdayHelper.Format(birthdays, Configuration);
        await channel.SendMessageAsync(formatted);
        context.Result = $"Finished (Birthdays: {birthdays.Count})";
    }
}
