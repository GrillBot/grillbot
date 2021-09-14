using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Handlers;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Birthday;
using GrillBot.App.Services.FileStorage;
using GrillBot.App.Services.Logging;
using GrillBot.App.Services.MessageCache;
using GrillBot.App.Services.Reminder;
using GrillBot.App.Services.Sync;
using GrillBot.App.Services.Unverify;
using GrillBot.Data.Helpers;
using GrillBot.Database;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using System;
using System.Linq;
using System.Text;
using Quartz;
using GrillBot.App.Extensions;

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
                LogLevel = LogSeverity.Verbose
            };

            services
                .AddSingleton(new DiscordSocketClient(discordConfig))
                .AddSingleton(new CommandService(commandsConfig))
                .AddSingleton<DiscordSyncService>()
                .AddSingleton<LoggingService>()
                .AddSingleton<MessageCache>()
                .AddSingleton<FileStorageFactory>()
                .AddSingleton<RandomizationService>()
                .AddDatabase(connectionString)
                .AddMemoryCache()
                .AddControllers()
                .AddNewtonsoftJson();

            services
                .AddSingleton<InviteService>()
                .AddSingleton<AutoReplyService>()
                .AddSingleton<ChannelService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<ReactionHandler>()
                .AddSingleton<AuditLogService>()
                .AddSingleton<PointsService>()
                .AddSingleton<EmoteService>()
                .AddSingleton<EmoteChainService>()
                .AddSingleton<SearchingService>()
                .AddSingleton<RemindService>()
                .AddSingleton<BirthdayService>()
                .AddUnverify()
                .AddSingleton<BoosterService>()
                .AddSingleton<OAuth2Service>();

            ReflectionHelper.GetAllReactionEventHandlers().ToList()
                .ForEach(o => services.AddSingleton(typeof(ReactionEventHandler), o));

            services.AddHttpClient("MathJS", c =>
            {
                c.BaseAddress = new Uri(Configuration["Math:Api"]);
                c.Timeout = TimeSpan.FromMilliseconds(Convert.ToInt32(Configuration["Math:Timeout"]));
            });

            services.AddHttpClient("IsKachnaOpen", c =>
            {
                c.BaseAddress = new Uri(Configuration["IsKachnaOpen:Api"]);
                c.Timeout = TimeSpan.FromMilliseconds(Convert.ToInt32(Configuration["IsKachnaOpen:Timeout"]));
            });

            services.AddHostedService<DiscordService>();

            services.AddOpenApiDocument(doc =>
            {
                doc.AddSecurity(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme()
                {
                    BearerFormat = "JWT",
                    Description = "JWT Authentication token",
                    Name = "JWT",
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Type = OpenApiSecuritySchemeType.Http,
                    In = OpenApiSecurityApiKeyLocation.Header
                });

                doc.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor(JwtBearerDefaults.AuthenticationScheme));

                doc.PostProcess = document =>
                {
                    document.Info = new OpenApiInfo
                    {
                        Title = "GrillBot",
                        Description = "Discord bot primarly for VUT FIT Discord server",
                        Version = "v1",

                        License = new OpenApiLicense
                        {
                            Name = "All rights reserved",
                            Url = "https://gist.github.com/Techcable/e7bbc22ecbc0050efbcc"
                        }
                    };
                };
            });

            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                q.AddTriggeredJob<MessageCacheCheckCron>(Configuration, "Discord:MessageCache:Period");
                q.AddTriggeredJob<AuditLogClearingJob>(Configuration, "AuditLog:CleaningCron");
                q.AddTriggeredJob<RemindCronJob>(Configuration, "Reminder:CronJob");
                q.AddTriggeredJob<BirthdayCronJob>(Configuration, "Birthday:Cron");
            });

            services.AddQuartzHostedService();

            services.AddAuthorization()
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.SaveToken = true;
                    o.IncludeErrorDetails = true;

                    var machineInfo = $"{Environment.MachineName}/{Environment.UserName}";
                    o.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = $"GrillBot/Issuer/{machineInfo}",
                        ValidAudience = $"GrillBot/Audience/{machineInfo}",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes($"{Configuration["OAuth2:ClientId"]}_{Configuration["OAuth2:ClientSecret"]}"))
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, GrillBotContext db)
        {
            if (db.Database.GetPendingMigrations().Any())
                db.Database.Migrate();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();

            app.UseOpenApi();
            app.UseSwaggerUi3(settings =>
            {
                settings.TransformToExternalPath = (route, request) =>
                {
                    string pathBase = request.Headers["X-Forwarded-PathBase"].FirstOrDefault();
                    return !string.IsNullOrEmpty(pathBase) ? pathBase + route : route;
                };
            });

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
