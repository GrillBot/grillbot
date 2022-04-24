using GrillBot.App.Infrastructure;
using GrillBot.App.Services.CommandsHelp.Parsers;
using GrillBot.App.Services.Discord;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Extensions;
using GrillBot.Data.Models.API.Help;
using GrillBot.Data.Models.DirectApi;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace GrillBot.App.Services.CommandsHelp;

/// <summary>
/// Service for generating commands help for external bots.
/// </summary>
[Initializable]
public class ExternalCommandsHelpService
{
    private DiscordSocketClient DiscordClient { get; }
    private IConfiguration Configuration { get; }
    private IMemoryCache MemoryCache { get; }
    private DiscordInitializationService InitializationService { get; }
    private IServiceProvider ServiceProvider { get; }

    private List<ulong> AllowedExternalServices { get; }
    private List<ulong> AuthorizedChannels { get; }
    private Type ParserInterfaceType { get; }

    public ExternalCommandsHelpService(DiscordSocketClient discordClient, IConfiguration configuration, IMemoryCache memoryCache,
        DiscordInitializationService initializationService, IServiceProvider serviceProvider)
    {
        DiscordClient = discordClient;
        Configuration = configuration.GetSection("Services");
        MemoryCache = memoryCache;
        InitializationService = initializationService;
        ServiceProvider = serviceProvider;

        DiscordClient.MessageReceived += OnMessageReceivedAsync;

        AllowedExternalServices = Configuration.AsEnumerable()
            .Where(o => o.Key.EndsWith(":Id"))
            .Select(o => o.Value.ToUlong())
            .Distinct()
            .ToList();

        AuthorizedChannels = Configuration.AsEnumerable()
            .Where(o => o.Key.EndsWith(":AuthorizedChannelId"))
            .Select(o => o.Value.ToUlong())
            .Distinct()
            .ToList();

        ParserInterfaceType = typeof(IHelpParser);
    }

    private async Task OnMessageReceivedAsync(SocketMessage message)
    {
        if (
            !InitializationService.Get() ||
            !AllowedExternalServices.Contains(message.Author.Id) ||
            message.Reference == null ||
            message.Attachments.Count != 1 ||
            !AuthorizedChannels.Contains(message.Channel.Id)
        )
        {
            return;
        }

        var attachmentData = await message.Attachments.First().DownloadAsync();
        var json = Encoding.UTF8.GetString(attachmentData);

        MemoryCache.Set(GetCacheKey(message.Reference.MessageId.Value), json, DateTimeOffset.UtcNow.AddMinutes(10));
    }

    public async Task<List<CommandGroup>> GetHelpAsync(string service, ulong loggedUserId, CancellationToken cancellationToken)
    {
        var configuration = FindServiceConfiguration(service);

        var commandRequest = await SendCommandHelpRequestAsync(configuration, loggedUserId, cancellationToken);
        var json = await WaitAndGetResponseAsync(configuration, commandRequest, cancellationToken);
        var data = JArray.Parse(json);

        return FindParserAndParse(configuration, data);
    }

    private IConfiguration FindServiceConfiguration(string service)
    {
        var configuration = Configuration.GetSection(service);

        if (!configuration.Exists())
            throw new GrillBotException($"Cannot find configuration for external service {service}");

        return configuration;
    }

    private async Task<IUserMessage> SendCommandHelpRequestAsync(IConfiguration externalServiceConfig, ulong loggedUserId, CancellationToken cancellationToken)
    {
        var directApiChannelId = externalServiceConfig.GetValue<ulong>("AuthorizedChannelId");
        var directApiChannel = DiscordClient.FindTextChannel(directApiChannelId);
        if (directApiChannel == null)
            throw new GrillBotException("Cannot find authorized direct API channel.");

        var command = new DirectMessageCommand("help");
        command.Parameters.Add("user_id", loggedUserId);
        var json = JsonConvert.SerializeObject(command);

        return await directApiChannel.SendMessageAsync(
            $"```json\n{json}\n```",
            options: new RequestOptions() { CancelToken = cancellationToken }
        );
    }

    private string GetCacheKey(ulong messageId)
        => $"{nameof(ExternalCommandsHelpService)}_{messageId}";

    private async Task<string> WaitAndGetResponseAsync(IConfiguration serviceConfiguration, IUserMessage request, CancellationToken cancellationToken)
    {
        var timeout = serviceConfiguration.GetValue<double>("Timeout");
        var timeoutChecks = serviceConfiguration.GetValue<int>("TimeoutChecks");
        var delay = timeout / timeoutChecks;

        var cacheKey = GetCacheKey(request.Id);
        string json = null;

        for (int i = 0; i < timeoutChecks; i++)
        {
            await Task.Delay(Convert.ToInt32(delay), cancellationToken);

            if (MemoryCache.TryGetValue(cacheKey, out json))
                break;
        }

        if (string.IsNullOrEmpty(json))
            throw new GrillBotException("Cannot get response. The external service did not respond within the expected limit. Try again later please.");

        return json;
    }

    private List<CommandGroup> FindParserAndParse(IConfiguration externalServiceConfig, JArray json)
    {
        IHelpParser parserInstance = null;
        var parserName = externalServiceConfig.GetValue<string>("ParserClass");

        if (!string.IsNullOrEmpty(parserName))
        {
            var parserType = Array.Find(
                Assembly.GetExecutingAssembly().GetTypes(),
                o => o.GetInterface(ParserInterfaceType.Name) != null && o.Name == parserName
            );

            if (parserType != null)
            {
                parserInstance = ServiceProvider.GetService(parserType) as IHelpParser;
                if (parserInstance == null)
                {
                    var constructor = parserType.GetConstructor(Type.EmptyTypes);

                    if (constructor?.IsPublic == true && constructor.GetParameters().Length == 0)
                        parserInstance = Activator.CreateInstance(parserType) as IHelpParser;
                }
            }
        }

        // If no parser is defined or cannot get instance, then is used direct deserialization to API models.
        if (parserInstance == null)
            return json.ToObject<List<CommandGroup>>();

        return parserInstance.Parse(json);
    }
}
