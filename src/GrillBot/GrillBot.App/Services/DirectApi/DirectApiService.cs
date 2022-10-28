using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.DirectApi;

namespace GrillBot.App.Services.DirectApi;

[Initializable]
public class DirectApiService : IDirectApiService
{
    private IConfiguration Configuration { get; }
    private DiscordSocketClient DiscordClient { get; }

    private List<ulong> AuthorizedServices { get; }

    public DirectApiService(DiscordSocketClient client, IConfiguration configuration)
    {
        Configuration = configuration.GetRequiredSection("Services");
        DiscordClient = client;

        AuthorizedServices = Configuration.AsEnumerable()
            .Where(o => o.Key.EndsWith(":Id"))
            .Select(o => o.Value.ToUlong())
            .Distinct()
            .ToList();
    }

    public async Task<string> SendCommandAsync(string service, DirectMessageCommand command)
    {
        var configuration = Configuration.GetRequiredSection(service);

        var request = await SendCommandRequestAsync(configuration, command);
        return await GetResponseAsync(configuration, request);
    }

    private async Task<IUserMessage> SendCommandRequestAsync(IConfiguration configuration, DirectMessageCommand command)
    {
        var channelId = configuration.GetValue<ulong>("AuthorizedChannelId");
        var apiChannel = await DiscordClient.FindTextChannelAsync(channelId);
        if (apiChannel == null)
            throw new GrillBotException("Cannot find authorized direct API channel");

        var json = JsonConvert.SerializeObject(command);
        return await apiChannel.SendMessageAsync($"```json\n{json}\n```");
    }

    private async Task<string> GetResponseAsync(IConfiguration configuration, IUserMessage request)
    {
        var timeout = configuration.GetValue<int>("Timeout");
        await Task.Delay(timeout);

        var messages = await request.Channel.GetMessagesAsync(mode: CacheMode.AllowDownload).FlattenAsync();
        var response = messages.FirstOrDefault(o => IsValidResponse(o, request));
        if (response == null) return null;

        var attachment = await response.Attachments.First().DownloadAsync();
        return attachment == null ? null : Encoding.UTF8.GetString(attachment);
    }

    private bool IsValidResponse(IMessage response, IUserMessage request)
    {
        return response != null && !response.Author.IsUser() && AuthorizedServices.Contains(response.Author.Id) && response.Reference is { MessageId.IsSpecified: true } &&
               response.Attachments.Count == 1 && response.Reference.MessageId.Value == request.Id;
    }
}
