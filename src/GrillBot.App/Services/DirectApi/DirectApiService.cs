using GrillBot.App.Helpers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.DirectApi;

namespace GrillBot.App.Services.DirectApi;

public class DirectApiService : IDirectApiService
{
    private IConfiguration Configuration { get; }
    private IDiscordClient DiscordClient { get; }
    private DownloadHelper DownloadHelper { get; }

    private List<ulong> AuthorizedServices { get; }

    public DirectApiService(IDiscordClient client, IConfiguration configuration, DownloadHelper downloadHelper)
    {
        Configuration = configuration.GetRequiredSection("Services");
        DiscordClient = client;
        DownloadHelper = downloadHelper;

        AuthorizedServices = Configuration.AsEnumerable()
            .Where(o => o.Key.EndsWith(":Id"))
            .Select(o => o.Value?.ToUlong() ?? 0)
            .Distinct()
            .ToList();
    }

    public async Task<string?> SendCommandAsync(string service, DirectMessageCommand command)
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

    private async Task<string?> GetResponseAsync(IConfiguration configuration, IUserMessage request)
    {
        var timeout = configuration.GetValue<int>("Timeout");
        var checks = configuration.GetValue<int>("Checks");
        var delay = Convert.ToInt32(timeout / checks);

        IMessage? message = null;
        for (var i = 0; i < checks; i++)
        {
            await Task.Delay(delay);

            var messages = await request.Channel.GetMessagesAsync(request.Id, Direction.After).FlattenAsync();
            var response = messages.FirstOrDefault(o => IsValidResponse(o, request));
            if (response == null) continue;

            message = response;
            break;
        }

        if (message == null) return null;
        var attachment = await DownloadHelper.DownloadAsync(message.Attachments.First());
        return attachment == null ? null : Encoding.UTF8.GetString(attachment);
    }

    private bool IsValidResponse(IMessage? response, IUserMessage request)
    {
        return response != null && !response.Author.IsUser() && AuthorizedServices.Contains(response.Author.Id) && response.Reference is { MessageId.IsSpecified: true } &&
               response.Attachments.Count == 1 && response.Reference.MessageId.Value == request.Id;
    }
}
