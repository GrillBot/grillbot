using GrillBot.App.Actions.Api.V2;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Data.Models.API;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class BirthdayCronJob(
    IConfiguration _configuration,
    IDiscordClient _discordClient,
    GetTodayBirthdayInfo _apiAction,
    GrillBotDatabaseBuilder _databaseBuilder,
    IServiceProvider serviceProvider
) : Job(serviceProvider)
{
    protected override async Task RunAsync(IJobExecutionContext context)
    {
        using var repository = _databaseBuilder.CreateRepository();
        if (!await repository.User.HaveSomeoneBirthdayTodayAsync())
            return;

        var birthdayNotificationSection = _configuration.GetSection("Birthday:Notifications");
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

        _apiAction.UpdateContext("cs", _discordClient.CurrentUser);
        var result = await _apiAction.ProcessAsync();
        var message = ((MessageResponse)result.Data!).Message;

        context.Result = message;
        await channel.SendMessageAsync(message, allowedMentions: AllowedMentions.None);
    }
}
