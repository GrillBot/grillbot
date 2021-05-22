using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services;
using GrillBot.Database;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reflection;

namespace GrillBot.App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("Default");

            var discordConfig = new DiscordSocketConfig()
            {
                GatewayIntents = DiscordHelper.GetAllIntents(),
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 50000,
                RateLimitPrecision = RateLimitPrecision.Millisecond
            };

            var commandsConfig = new CommandServiceConfig()
            {
                CaseSensitiveCommands = true,
                DefaultRunMode = RunMode.Async,
                IgnoreExtraArgs = true,
                LogLevel = LogSeverity.Verbose
            };

            services
                .AddSingleton(new DiscordSocketClient(discordConfig))
                .AddSingleton(new CommandService(commandsConfig))
                .AddSingleton<LoggingService>()
                .AddDatabase(connectionString)
                .AddControllers();

            ReflectionHelper.GetAllEventHandlers().ToList()
                .ForEach(o => services.AddSingleton(typeof(Handler), o));

            ReflectionHelper.GetAllReactionEventHandlers().ToList()
                .ForEach(o => services.AddSingleton(typeof(ReactionEventHandler), o));

            services.AddHostedService<DiscordService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, GrillBotContext db)
        {
            if (db.Database.GetPendingMigrations().Any())
                db.Database.Migrate();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
