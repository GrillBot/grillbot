using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using Quartz;

namespace GrillBot.App.Services.Birthday;

[DisallowConcurrentExecution]
public class BirthdayCronJob : Job
{
    private BirthdayService BirthdayService { get; }
    private IConfiguration Configuration { get; }

    public BirthdayCronJob(IConfiguration configuration, BirthdayService service, LoggingService logging,
        AuditLogService auditLogService, IDiscordClient discordClient) : base(logging, auditLogService, discordClient)
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
