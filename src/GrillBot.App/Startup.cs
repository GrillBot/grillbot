using System.Reflection;
using Discord.Interactions;
using GrillBot.App.Actions;
using GrillBot.App.Handlers;
using GrillBot.App.Services.Discord;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using Quartz;
using GrillBot.App.Infrastructure.OpenApi;
using GrillBot.App.Infrastructure.RequestProcessing;
using GrillBot.App.Jobs;
using GrillBot.App.Managers;
using GrillBot.Cache;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Common;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Services;
using GrillBot.Core;
using Microsoft.AspNetCore.HttpOverrides;
using GrillBot.Core.RabbitMQ;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using GrillBot.App.Infrastructure.JsonConverters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
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
            LogGatewayIntentWarnings = false,
            UseInteractionSnowflakeDate = false
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

        ManagersExtensions.AddLocalization(services, localizationPath, "messages");
        services.AddHelpers()
            .AddCommonManagers()
            .Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true)
            .AddSingleton(discordClient)
            .AddSingleton<IDiscordClient>(discordClient)
            .AddSingleton(container => new InteractionService(container.GetRequiredService<DiscordSocketClient>(), interactionsConfig))
            .AddCaching(Configuration)
            .AddDatabase(connectionString!)
            .AddMemoryCache()
            .AddActions()
            .AddSingleton<BlobManagerFactory>()
            .AddRabbitMQ()
            .AddControllers(c =>
            {
                c.Filters.Add<ExceptionFilter>();
                c.Filters.Add<RequestFilter>();
                c.Filters.Add<ResultFilter>();
                c.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

                c.CacheProfiles.Add("LookupListCache", new CacheProfile
                {
                    Duration = 5 * 60,
                    Location = ResponseCacheLocation.Client
                });
            })
            .AddNewtonsoftJson(setup => setup.SerializerSettings.Converters.Add(new SystemTextJsonToNewtonsoftConverter()));

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

        services
            .AddOpenApiDoc("v1", "WebAdmin API", "V1 API is only for web based administrations. If you're third party service, use API V2.", doc =>
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
            })
            .AddOpenApiDoc("v2", "Third-party API",
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
                }
            )
            .AddOpenApiDoc("v3", "WebAdmin API", "Second generation of API for web based administrations. If you're third party service, use API V2.", doc =>
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
            });

        services.AddQuartz(q =>
        {
            q.AddTriggeredJob<MessageCacheJob>(Configuration, "Discord:MessageCache:Period");
            q.AddTriggeredJob<AuditLogClearingJob>(Configuration, "AuditLog:CleaningCron");
            q.AddTriggeredJob<RemindCronJob>(Configuration, "Reminder:CronJob");
            q.AddTriggeredJob<BirthdayCronJob>(Configuration, "Birthday:Cron", true);
            q.AddTriggeredJob<UnverifyCronJob>(Configuration, "Unverify:CheckPeriodTime");
            q.AddTriggeredJob<UserSynchronizationJob>(Configuration, "UserSyncPeriodTime");
            q.AddTriggeredJob<PointsJob>(Configuration, "Points:JobInterval");
            q.AddTriggeredJob<CacheCleanerJob>(Configuration, "CacheCleanerInterval");
            q.AddTriggeredJob<UnverifyLogArchivationJob>(Configuration, "Unverify:LogArchivePeriod");
        });

        services.AddQuartzHostedService();

        services
            .AddAuthorization()
            .AddAuthentication(opt =>
            {
                opt.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
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
            })
            .AddOAuth("Discord", opt =>
            {
                opt.AuthorizationEndpoint = "https://discord.com/oauth2/authorize";
                opt.TokenEndpoint = "https://discord.com/api/oauth2/token";
                opt.UserInformationEndpoint = "https://discord.com/api/users/@me";
                opt.AccessDeniedPath = "/AuthView/DiscordAuthFailed";
                opt.ClientId = Configuration["Auth:OAuth2:ClientId"]!;
                opt.ClientSecret = Configuration["Auth:OAuth2:ClientSecret"]!;
                opt.CallbackPath = new PathString("/api/auth/oauth2callback");

                opt.Scope.Add("identify");

                opt.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                opt.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
                opt.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "global_name");

                opt.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                        using var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        var userJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                        context.RunClaimActions(userJson);
                    }
                };
            });

        services.AddHealthChecks()
            .AddCheck<DiscordHealthCheck>(nameof(DiscordHealthCheck))
            .AddNpgSql(connectionString!);

        services.Configure<ForwardedHeadersOptions>(opt => opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        app.InitDatabase<GrillBotContext>();
        app.InitCache();

        if (environment.IsProduction())
        {
            app.Use((ctx, next) =>
            {
                ctx.Request.Scheme = "https";
                return next();
            });
        }

        app.Use((context, next) =>
        {
            context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
            context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

            return next();
        });

        app.UseForwardedHeaders();
        var corsOrigins = Configuration.GetSection("CORS:Origins").AsEnumerable()
            .Select(o => o.Value).Where(o => !string.IsNullOrEmpty(o)).ToArray();
        app.UseCors(policy => policy.WithMethods("GET", "POST", "PUT", "DELETE", "PATCH").AllowAnyHeader().WithOrigins(corsOrigins!).AllowCredentials());

        app.UseResponseCaching();
        app.UseRouting();
        app.UseAuthorization();
        app.UseAuthentication();

        app.UseOpenApi();
        app.UseSwaggerUi(settings =>
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
