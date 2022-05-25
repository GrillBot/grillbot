using GrillBot.App.Infrastructure;
using GrillBot.Cache.Entity;
using GrillBot.Cache.Services;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.DirectApi;

namespace GrillBot.App.Services.DirectApi;

[Initializable]
public class DirectApiService : ServiceBase
{
    private IConfiguration Configuration { get; }
    private InitManager InitManager { get; }

    private List<ulong> AuthorizedChannelIds { get; }
    private List<ulong> AuthorizedServices { get; }

    public DirectApiService(DiscordSocketClient client, IConfiguration configuration,
        InitManager initManager, GrillBotCacheBuilder cacheBuilder) : base(client, null, null, null, cacheBuilder)
    {
        Configuration = configuration.GetRequiredSection("Services");
        InitManager = initManager;

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
        var entity = new DirectApiMessage()
        {
            ExpireAt = DateTime.UtcNow.AddMinutes(10),
            Id = message.Reference.MessageId.Value.ToString(),
            JsonData = Encoding.UTF8.GetString(attachmentData)
        };

        using var cache = CacheBuilder.CreateRepository();

        await cache.AddAsync(entity);
        await cache.CommitAsync();
    }

    private bool CanReceiveMessage(SocketMessage message)
        => InitManager.Get() && AuthorizedServices.Contains(message.Author.Id) && message.Reference != null && message.Attachments.Count == 1 &&
        AuthorizedChannelIds.Contains(message.Channel.Id);

    public async Task<string> SendCommandAsync(string service, DirectMessageCommand command, CancellationToken cancellationToken = default)
    {
        var configuration = Configuration.GetRequiredSection(service);

        var request = await SendCommandRequestAsync(configuration, command, cancellationToken);
        return await WaitAndGetResponseAsync(configuration, request);
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

    private async Task<string> WaitAndGetResponseAsync(IConfiguration configuration, IUserMessage request)
    {
        var timeout = configuration.GetValue<double>("Timeout");
        var timeoutChecks = configuration.GetValue<int>("TimeoutChecks");
        var delay = timeout / timeoutChecks;

        DirectApiMessage msg = null;
        for (int i = 0; i < timeoutChecks; i++)
        {
            await Task.Delay(Convert.ToInt32(delay));

            msg = await TryGetCachedMessage(request);
            if (msg != null)
                break;
        }

        if (msg == null)
            throw new GrillBotException("Cannot get response. The external service did not respond within the expected limit. Try again later please.");

        return msg.JsonData;
    }

    private async Task<DirectApiMessage> TryGetCachedMessage(IUserMessage message)
    {
        using var cache = CacheBuilder.CreateRepository();

        var msg = await cache.DirectApiRepository.FindMessageByIdAsync(message.Id);

        if (msg != null)
        {
            cache.Remove(msg);
            await cache.CommitAsync();
        }

        return msg;
    }
}
