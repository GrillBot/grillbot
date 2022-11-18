using GrillBot.App.Actions.Api.V2;
using GrillBot.App.Infrastructure.Jobs;
using Quartz;

namespace GrillBot.App.Services.Birthday;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class BirthdayCronJob : Job
{
    private IConfiguration Configuration { get; }
    private GetTodayBirthdayInfo GetTodayBirthdayInfo { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public BirthdayCronJob(IConfiguration configuration, IDiscordClient discordClient, GetTodayBirthdayInfo getTodayBirthdayInfo, GrillBotDatabaseBuilder databaseBuilder,
        IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configuration = configuration;
        GetTodayBirthdayInfo = getTodayBirthdayInfo;
        GetTodayBirthdayInfo.UpdateContext("cs", discordClient.CurrentUser);
        DatabaseBuilder = databaseBuilder;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        if (!await repository.User.HaveSomeoneBirthdayTodayAsync())
            return;

        var birthdayNotificationSection = Configuration.GetSection("Birthday:Notifications");
        var guild = await DiscordClient.GetGuildAsync(birthdayNotificationSection.GetValue<ulong>("GuildId"));
        if (guild == null)
        {
            context.Result = "Required guild for birthdays wasn't found.";
            return;
        }

        var channel = await guild.GetTextChannelAsync(birthdayNotificationSection.GetValue<ulong>("ChannelId"));
        if (channel == null)
        {
            context.Result = "Required channel for birthdays wasn't found.";
            return;
        }

        var result = await GetTodayBirthdayInfo.ProcessAsync();
        context.Result = result;

        await channel.SendMessageAsync(result);
    }
}
