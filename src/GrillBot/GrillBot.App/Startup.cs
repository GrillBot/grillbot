using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.Data.Handlers;
using GrillBot.Data.Helpers;
using GrillBot.Data.Infrastructure;
using GrillBot.Data.Services;
using GrillBot.Data.Services.AuditLog;
using GrillBot.Data.Services.Birthday;
using GrillBot.Data.Services.FileStorage;
using GrillBot.Data.Services.Logging;
using GrillBot.Data.Services.MessageCache;
using GrillBot.Data.Services.Reminder;
using GrillBot.Data.Services.Unverify;
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
using GrillBot.Data.Extensions;
using GrillBot.Data.Services.Discord;
using Discord.Interactions;
using GrillBot.Data.Services.Emotes;

namespace GrillBot.Data;

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
            GatewayIntents = GatewayIntents.All,
            LogLevel = LogSeverity.Verbose,
            MessageCacheSize = 5000,
            AlwaysDownloadDefaultStickers = true,
            AlwaysDownloadUsers = true,
            AlwaysResolveStickers = true,
            LogGatewayIntentWarnings = false
        };

        var commandsConfig = new CommandServiceConfig()
        {
            CaseSensitiveCommands = true,
            DefaultRunMode = Discord.Commands.RunMode.Async,
            LogLevel = LogSeverity.Verbose
        };

        var interactionsConfig = new InteractionServiceConfig()
        {
            DefaultRunMode = Discord.Interactions.RunMode.Async,
            EnableAutocompleteHandlers = true,
            LogLevel = LogSeverity.Verbose,
            UseCompiledLambda = true
        };

        services
            .AddSingleton(new DiscordSocketClient(discordConfig))
            .AddSingleton(new CommandService(commandsConfig))
            .AddSingleton(container => new InteractionService(container.GetRequiredService<DiscordSocketClient>(), interactionsConfig))
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
            .AddSingleton<OAuth2Service>()
            .AddSingleton<DiscordInitializationService>()
            .AddSingleton<MockingService>()
            .AddSingleton<InteractionHandler>();

        ReflectionHelper.GetAllReactionEventHandlers().ToList()
            .ForEach(o => services.AddSingleton(typeof(ReactionEventHandler), o));

        services.AddHttpClient("MathJS", c =>
        {
            c.BaseAddress = new Uri(Configuration["Math:Api"]);
            c.Timeout = TimeSpan.FromMilliseconds(Convert.ToInt32(Configuration["Math:Timeout"]));
        });

        services.AddHttpClient("KachnaOnline", c =>
        {
            c.BaseAddress = new Uri(Configuration["KachnaOnline:Api"]);
            c.Timeout = TimeSpan.FromMilliseconds(Convert.ToInt32(Configuration["KachnaOnline:Timeout"]));
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
            q.AddTriggeredJob<UnverifyCronJob>(Configuration, "Unverify:CheckPeriodTime");
            q.AddTriggeredJob<OnlineUsersCleanJob>(Configuration, "OnlineUsersCheckPeriodTime");
            q.AddTriggeredJob<EmoteStatsCleaningJob>(Configuration, "Emotes:CleaningInterval");
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

        services.AddHealthChecks()
            .AddCheck<DiscordHealthCheck>(nameof(DiscordHealthCheck))
            .AddNpgSql(connectionString);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, GrillBotContext db)
    {
        if (db.Database.GetPendingMigrations().Any())
            db.Database.Migrate();

        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();

        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseCors(policy => policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());
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

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health");
        });
    }
}
