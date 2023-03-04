using System.Reflection;
using Discord.Interactions;
using GrillBot.App.Actions;
using GrillBot.App.Handlers;
using GrillBot.App.Services.Discord;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using Quartz;
using GrillBot.App.Infrastructure.OpenApi;
using GrillBot.App.Infrastructure.RequestProcessing;
using GrillBot.App.Jobs;
using GrillBot.App.Managers;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Cache;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Common;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Services;
using Microsoft.AspNetCore.HttpOverrides;

namespace GrillBot.App;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = Configuration.GetConnectionString("Default");

        var discordConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All,
            LogLevel = LogSeverity.Verbose,
            MessageCacheSize = 5000,
            AlwaysDownloadDefaultStickers = true,
            AlwaysDownloadUsers = true,
            AlwaysResolveStickers = true,
            DefaultRetryMode = RetryMode.RetryRatelimit,
            SuppressUnknownDispatchWarnings = true,
            LogGatewayIntentWarnings = false
        };

        var currentAssembly = Assembly.GetExecutingAssembly();
        var basePath = Path.GetDirectoryName(currentAssembly.Location)!;
        var localizationPath = Path.Combine(basePath, "Resources", "Localization");

        var interactionsConfig = new InteractionServiceConfig
        {
            DefaultRunMode = RunMode.Async,
            EnableAutocompleteHandlers = true,
            LogLevel = LogSeverity.Verbose,
            UseCompiledLambda = true,
            LocalizationManager = new JsonLocalizationManager(localizationPath, "commands"),
            AutoServiceScopes = true
        };

        var discordClient = new DiscordSocketClient(discordConfig);

        services.AddHelpers()
            .AddCommonManagers()
            .AddLocalization(localizationPath, "messages")
            .Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true)
            .AddSingleton(discordClient)
            .AddSingleton<IDiscordClient>(discordClient)
            .AddSingleton(container => new InteractionService(container.GetRequiredService<DiscordSocketClient>(), interactionsConfig))
            .AddCaching(Configuration)
            .AddDatabase(connectionString!)
            .AddMemoryCache()
            .AddScoped<ApiRequest>()
            .AddActions()
            .AddSingleton<FileStorageFactory>()
            .AddControllers(c =>
            {
                c.Filters.Add<ExceptionFilter>();
                c.Filters.Add<RequestFilter>();
                c.Filters.Add<ResultFilter>();
            })
            .AddNewtonsoftJson();

        var referencedAssemblies = currentAssembly
            .GetReferencedAssemblies()
            .Where(o => o.Name!.StartsWith("GrillBot"))
            .Select(Assembly.Load)
            .ToArray();
        services.AddAutoMapper(new[] { new[] { currentAssembly }, referencedAssemblies }.SelectMany(o => o));

        services
            .AddHandlers()
            .AddManagers();
        Helpers.ServiceExtensions.AddHelpers(services);

        services.AddHostedService<DiscordService>();
        services.AddThirdPartyServices(Configuration);

        services.AddOpenApiDoc("v1", "WebAdmin API", "V1 API is only for web based administrations. If you're third party service, use API V2.", doc =>
        {
            doc.AddSecurity(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Description = "JWT Authentication token",
                Name = "JWT",
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Type = OpenApiSecuritySchemeType.Http,
                In = OpenApiSecurityApiKeyLocation.Header
            });

            doc.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor(JwtBearerDefaults.AuthenticationScheme));
        }).AddOpenApiDoc("v2", "Third-party API",
            "API for third party application with ApiKey authentication. Any implementation of new endpoints is based on requirements. New requirement or access request (to obtain API key) you " +
            "can submit via issues on github or in #bot-development channel if you're in the VUT FIT discord guild.",
            doc =>
            {
                doc.AddSecurity("ApiKey", new OpenApiSecurityScheme
                {
                    Name = "ApiKey",
                    Scheme = "ApiKey",
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    In = OpenApiSecurityApiKeyLocation.Header
                });

                doc.OperationProcessors.Add(new ApiKeyAuthProcessor());
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
                o.TokenValidationParameters = new TokenValidationParameters
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
            .AddNpgSql(connectionString!);

        services.Configure<ForwardedHeadersOptions>(opt => opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.InitDatabase();
        app.InitCache();

        app.UseForwardedHeaders();
        var corsOrigins = Configuration.GetSection("CORS:Origins").AsEnumerable()
            .Select(o => o.Value).Where(o => !string.IsNullOrEmpty(o)).ToArray();
        app.UseCors(policy => policy.AllowAnyMethod().AllowAnyHeader().WithOrigins(corsOrigins!));

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
