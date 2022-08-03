using System.Reflection;
using Discord.Commands;
using Discord.Interactions;
using GrillBot.App.Handlers;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Birthday;
using GrillBot.App.Services.Discord;
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
using GrillBot.App.Services.Suggestion;
using GrillBot.App.Infrastructure.OpenApi;
using GrillBot.App.Infrastructure.RequestProcessing;
using GrillBot.App.Services.User.Points;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Cache;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Common;
using GrillBot.Common.Extensions;

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
            DefaultRetryMode = RetryMode.RetryRatelimit,
            SuppressUnknownDispatchWarnings = true
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
            .AddCommonManagers()
            .Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true)
            .AddSingleton(discordClient)
            .AddSingleton<IDiscordClient>(discordClient)
            .AddSingleton(new CommandService(commandsConfig))
            .AddSingleton(container => new InteractionService(container.GetRequiredService<DiscordSocketClient>(), interactionsConfig))
            .AddCaching(Configuration)
            .AddDatabase(connectionString)
            .AddMemoryCache()
            .AddAutoMapper(typeof(Startup).Assembly, typeof(Emojis).Assembly, typeof(GrillBotContext).Assembly)
            .AddScoped<ApiRequest>()
            .AddControllers(c =>
            {
                c.Filters.Add<ExceptionFilter>();
                c.Filters.Add<RequestFilter>();
                c.Filters.Add<ResultFilter>();

                c.CacheProfiles.Add("BoardApi", new CacheProfile { Duration = 60 }); // Response caching for boards (leaderboard, help, ...).
                c.CacheProfiles.Add("ConstsApi", new CacheProfile { Duration = 30 }); // Response caching for constants.
            })
            .AddNewtonsoftJson();

        var currentAssembly = Assembly.GetExecutingAssembly();
        var referencedAssemblies = currentAssembly
            .GetReferencedAssemblies()
            .Where(o => o.Name!.StartsWith("GrillBot"))
            .Select(Assembly.Load)
            .ToArray();
        services.AddAutoMapper(new[] { new[] { currentAssembly }, referencedAssemblies }.SelectMany(o => o));

        services
            .AddSingleton<CommandHandler>()
            .AddSingleton<ReactionHandler>()
            .AddSingleton<InteractionHandler>()
            .AddServices();

        var reactionHandlerType = typeof(ReactionEventHandler);
        reactionHandlerType.Assembly.GetTypes()
            .Where(o => !o.IsAbstract && reactionHandlerType.IsAssignableFrom(o))
            .ToList()
            .ForEach(o => services.AddSingleton(typeof(ReactionEventHandler), o));

        services.AddHttpClient("MathJS", c =>
        {
            c.BaseAddress = new Uri(Configuration["Services:Math:Api"]);
            c.Timeout = TimeSpan.FromMilliseconds(Configuration["Services:Math:Timeout"].ToInt());
        });

        services.AddHttpClient("KachnaOnline", c =>
        {
            c.BaseAddress = new Uri(Configuration["Services:KachnaOnline:Api"]);
            c.Timeout = TimeSpan.FromMilliseconds(Configuration["Services:KachnaOnline:Timeout"].ToInt());
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

            doc.AddSecurity("ApiKey", new OpenApiSecurityScheme()
            {
                Name = "ApiKey",
                Scheme = "ApiKey",
                Type = OpenApiSecuritySchemeType.ApiKey,
                In = OpenApiSecurityApiKeyLocation.Header
            });

            doc.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor(JwtBearerDefaults.AuthenticationScheme));
            doc.OperationProcessors.Add(new OperationIdProcessor());
            doc.OperationProcessors.Add(new ApiKeyAuthProcessor());
            doc.OperationProcessors.Add(new OnlyDevelopmentProcessor());

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

            q.AddTriggeredJob<MessageCacheJob>(Configuration, "Discord:MessageCache:Period");
            q.AddTriggeredJob<AuditLogClearingJob>(Configuration, "AuditLog:CleaningCron");
            q.AddTriggeredJob<RemindCronJob>(Configuration, "Reminder:CronJob");
            q.AddTriggeredJob<BirthdayCronJob>(Configuration, "Birthday:Cron", true);
            q.AddTriggeredJob<UnverifyCronJob>(Configuration, "Unverify:CheckPeriodTime");
            q.AddTriggeredJob<OnlineUsersCleanJob>(Configuration, "OnlineUsersCheckPeriodTime");
            q.AddTriggeredJob<SuggestionJob>(Configuration, "SuggestionsCleaningInterval");
            q.AddTriggeredJob<PointsJob>(Configuration, "Points:JobInterval");
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes($"{Configuration["Auth:OAuth2:ClientId"]}_{Configuration["Auth:OAuth2:ClientSecret"]}"))
                };
            });

        services.AddHealthChecks()
            .AddCheck<DiscordHealthCheck>(nameof(DiscordHealthCheck))
            .AddNpgSql(connectionString);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.InitDatabase();
        app.InitCache();

        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();

        var corsOrigins = Configuration.GetSection("CORS:Origins").AsEnumerable()
            .Select(o => o.Value).Where(o => !string.IsNullOrEmpty(o)).ToArray();
        app.UseCors(policy => policy.AllowAnyMethod().AllowAnyHeader().WithOrigins(corsOrigins));

        app.UseResponseCaching();
        app.UseRouting();
        app.UseAuthorization();
        app.UseAuthentication();

        app.UseOpenApi();
        app.UseSwaggerUi3(settings =>
        {
            settings.TransformToExternalPath = (route, request) =>
            {
                var pathBase = request.Headers["X-Forwarded-PathBase"].FirstOrDefault();
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
