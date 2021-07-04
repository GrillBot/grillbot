using Discord.WebSocket;
using GrillBot.App.Services.CronJobs;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.App.Services.Birthday
{
    public class BirthdayCronJob : CronJobTask
    {
        private BirthdayService BirthdayService { get; }
        private IConfiguration Configuration { get; }

        public BirthdayCronJob(IConfiguration configuration, DiscordSocketClient discordClient,
            BirthdayService service) : base(configuration["Birthday:Cron"], discordClient)
        {
            BirthdayService = service;
            Configuration = configuration;
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var birthdays = await BirthdayService.GetTodayBirthdaysAsync(cancellationToken);
            if (birthdays.Count == 0) return;

            var birthdayNotificationSection = Configuration.GetSection("Birthday:Notifications");
            var guild = DiscordClient.GetGuild(birthdayNotificationSection.GetValue<ulong>("GuildId"));
            if (guild == null) return;

            var channel = guild.GetTextChannel(birthdayNotificationSection.GetValue<ulong>("ChannelId"));
            if (channel == null) return;

            var formatted = BirthdayHelper.Format(birthdays, Configuration);
            await channel.SendMessageAsync(formatted);
        }
    }
}
