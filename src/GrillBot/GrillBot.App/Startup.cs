using Discord.Commands;
using Discord.Interactions;
using GrillBot.App.Extensions;
using GrillBot.App.Handlers;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Birthday;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.Emotes;
using GrillBot.App.Services.FileStorage;
using GrillBot.App.Services.CommandsHelp;
using GrillBot.App.Services.Logging;
using GrillBot.App.Services.MessageCache;
using GrillBot.App.Services.Permissions;
using GrillBot.App.Services.Reminder;
using GrillBot.App.Services.Unverify;
using GrillBot.App.Services.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using Quartz;
using GrillBot.App.Services.AutoReply;

namespace GrillBot.App;

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
            LogGatewayIntentWarnings = false,
            DefaultRetryMode = RetryMode.RetryRatelimit
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

        var discordClient = new DiscordSocketClient(discordConfig);

        services
            .AddSingleton(discordClient)
            .AddSingleton<IDiscordClient>(discordClient)
            .AddSingleton(new CommandService(commandsConfig))
            .AddSingleton(container => new InteractionService(container.GetRequiredService<DiscordSocketClient>(), interactionsConfig))
            .AddSingleton<DiscordSyncService>()
            .AddSingleton<LoggingService>()
            .AddSingleton<MessageCache>()
            .AddSingleton<FileStorageFactory>()
            .AddSingleton<RandomizationService>()
            .AddDatabase(connectionString)
            .AddMemoryCache()
            .AddControllers(c => c.Filters.Add<OperationCancelledExceptionFilterAttribute>())
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
            .AddSingleton<InteractionHandler>()
            .AddCommandsHelp()
            .AddSingleton<PermissionsService>()
            .AddSingleton<UserService>()
            .AddSingleton<GuildService>()
            .AddServices();

        services
            .AddAutoMapper(typeof(Startup).Assembly, typeof(Emojis).Assembly, typeof(GrillBotContext).Assembly);

        ReflectionHelper.GetAllReactionEventHandlers().ToList()
            .ForEach(o => services.AddSingleton(typeof(ReactionEventHandler), o));

        services.AddHttpClient("MathJS", c =>
        {
            c.BaseAddress = new Uri(Configuration["Services:Math:Api"]);
            c.Timeout = TimeSpan.FromMilliseconds(Convert.ToInt32(Configuration["Services:Math:Timeout"]));
        });

        services.AddHttpClient("KachnaOnline", c =>
        {
            c.BaseAddress = new Uri(Configuration["Services:KachnaOnline:Api"]);
            c.Timeout = TimeSpan.FromMilliseconds(Convert.ToInt32(Configuration["Services:KachnaOnline:Timeout"]));
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
            q.AddTriggeredJob<BirthdayCronJob>(Configuration, "Birthday:Cron", true);
            q.AddTriggeredJob<UnverifyCronJob>(Configuration, "Unverify:CheckPeriodTime");
            q.AddTriggeredJob<OnlineUsersCleanJob>(Configuration, "OnlineUsersCheckPeriodTime");
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
                    ValidIssuer = $"GrillBot/{machineInfo}",
                    ValidAudience = $"GrillBot/{machineInfo}",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes($"{Configuration["OAuth2:ClientId"]}_{Configuration["OAuth2:ClientSecret"]}"))
                };
            });

        services.AddHealthChecks()
            .AddCheck<DiscordHealthCheck>(nameof(DiscordHealthCheck))
            .AddNpgSql(connectionString);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, GrillBotContext db, MessageCache messageCache)
    {
        if (db.Database.GetPendingMigrations().Any())
            db.Database.Migrate();

        messageCache.ClearIndexes();

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
