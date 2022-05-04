using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Discord;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Extensions;
using GrillBot.Data.Models.DirectApi;
using Microsoft.Extensions.Caching.Memory;
using System.Web;

namespace GrillBot.App.Services.DirectApi;

[Initializable]
public class DirectApiService : ServiceBase
{
    private IMemoryCache Cache { get; }
    private IConfiguration Configuration { get; }

    private List<ulong> AuthorizedChannelIds { get; }
    private List<ulong> AuthorizedServices { get; }

    public DirectApiService(DiscordSocketClient client, IConfiguration configuration, IMemoryCache cache,
        DiscordInitializationService initializationService) : base(client, null, initializationService, null, null)
    {
        Configuration = configuration.GetRequiredSection("Services");
        Cache = cache;

        AuthorizedChannelIds = Configuration.AsEnumerable()
            .Where(o => o.Key.EndsWith(":AuthorizedChannelId"))
            .Select(o => o.Value.ToUlong())
            .Distinct()
            .ToList();

        AuthorizedServices = Configuration.AsEnumerable()
            .Where(o => o.Key.EndsWith(":Id"))
            .Select(o => o.Value.ToUlong())
            .Distinct()
            .ToList();

        DiscordClient.MessageReceived += OnMessageReceivedAsync;
    }

    private async Task OnMessageReceivedAsync(SocketMessage message)
    {
        if (!CanReceiveMessage(message))
            return;

        var attachmentData = await message.Attachments.First().DownloadAsync();
        var json = Encoding.UTF8.GetString(attachmentData);

        Cache.Set(
            GetCacheKey(message.Reference.MessageId.Value),
            json,
            DateTimeOffset.UtcNow.AddMinutes(10)
        );
    }

    private bool CanReceiveMessage(SocketMessage message)
        => InitializationService.Get() && AuthorizedServices.Contains(message.Author.Id) && message.Reference != null && message.Attachments.Count == 1 &&
        AuthorizedChannelIds.Contains(message.Channel.Id);

    private string GetCacheKey(ulong messageId) => $"{nameof(DirectApiService)}_{messageId}";

    public async Task<string> SendCommandAsync(string service, DirectMessageCommand command, CancellationToken cancellationToken = default)
    {
        var configuration = Configuration.GetRequiredSection(service);

        var request = await SendCommandRequestAsync(configuration, command, cancellationToken);
        return await WaitAndGetResponseAsync(configuration, request, cancellationToken);
    }

    private async Task<IUserMessage> SendCommandRequestAsync(IConfiguration configuration, DirectMessageCommand command, CancellationToken cancellationToken = default)
    {
        var channelId = configuration.GetValue<ulong>("AuthorizedChannelId");
        var apiChannel = await DiscordClient.FindTextChannelAsync(channelId);
        if (apiChannel == null)
            throw new GrillBotException("Cannot find authorized direct API channel");

        var json = JsonConvert.SerializeObject(command);

        return await apiChannel.SendMessageAsync(
            $"```json\n{json}\n```",
            options: new() { CancelToken = cancellationToken }
        );
    }

    private async Task<string> WaitAndGetResponseAsync(IConfiguration configuration, IUserMessage request, CancellationToken cancellationToken = default)
    {
        var timeout = configuration.GetValue<double>("Timeout");
        var timeoutChecks = configuration.GetValue<int>("TimeoutChecks");
        var delay = timeout / timeoutChecks;

        var cacheKey = GetCacheKey(request.Id);
        string json = null;

        for (int i = 0; i < timeoutChecks; i++)
        {
            await Task.Delay(Convert.ToInt32(delay), cancellationToken);

            if (Cache.TryGetValue(cacheKey, out json))
                break;
        }

        if (string.IsNullOrEmpty(json))
            throw new GrillBotException("Cannot get response. The external service did not respond within the expected limit. Try again later please.");

        return json;
    }
}
