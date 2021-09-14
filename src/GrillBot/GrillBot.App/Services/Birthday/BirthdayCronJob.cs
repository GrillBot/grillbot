using Discord.WebSocket;
using GrillBot.App.Services.Logging;
using Microsoft.Extensions.Configuration;
using Quartz;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Services.Birthday
{
    [DisallowConcurrentExecution]
    public class BirthdayCronJob : IJob
    {
        private BirthdayService BirthdayService { get; }
        private IConfiguration Configuration { get; }
        private DiscordSocketClient DiscordClient { get; }
        private LoggingService Logging { get; }

        public BirthdayCronJob(IConfiguration configuration, DiscordSocketClient discordClient, BirthdayService service,
            LoggingService logging)
        {
            BirthdayService = service;
            Configuration = configuration;
            DiscordClient = discordClient;
            Logging = logging;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await Logging.InfoAsync("BirthdayCron", $"Triggered birthday processing job at {DateTime.Now}");

                var birthdays = await BirthdayService.GetTodayBirthdaysAsync(context.CancellationToken);
                if (birthdays.Count == 0) return;

                var birthdayNotificationSection = Configuration.GetSection("Birthday:Notifications");
                var guild = DiscordClient.GetGuild(birthdayNotificationSection.GetValue<ulong>("GuildId"));
                if (guild == null) return;

                var channel = guild.GetTextChannel(birthdayNotificationSection.GetValue<ulong>("ChannelId"));
                if (channel == null) return;

                var formatted = BirthdayHelper.Format(birthdays, Configuration);
                await channel.SendMessageAsync(formatted);
            }
            catch (Exception ex)
            {
                await Logging.ErrorAsync("BirthdayCron", "An error occured when birthday processing.", ex);
            }
        }
    }
}
