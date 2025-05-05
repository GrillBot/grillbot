using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.RabbitMQ.V2.Consumer;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.GrillBot.Models.Events.Messages;
using GrillBot.Core.Services.RemindService.Models.Events;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ.Messages;

public class CreatedDiscordMessageEventHandler : RabbitMessageHandlerBase<CreatedDiscordMessagePayload>
{
    private readonly IRabbitPublisher _rabbitPublisher;

    public CreatedDiscordMessageEventHandler(ILoggerFactory loggerFactory, IRabbitPublisher rabbitPublisher) : base(loggerFactory)
    {
        _rabbitPublisher = rabbitPublisher;
    }

    protected override async Task<RabbitConsumptionResult> HandleInternalAsync(CreatedDiscordMessagePayload message, ICurrentUserProvider currentUser, Dictionary<string, string> headers)
    {
        switch (message.ServiceId)
        {
            case "Remind":
                await ProcessRemindServiceMessageAsync(message);
                break;
        }

        return RabbitConsumptionResult.Success;
    }

    private async Task ProcessRemindServiceMessageAsync(CreatedDiscordMessagePayload payload)
    {
        if (payload.ServiceData.TryGetValue("RemindId", out var _remindId) && long.TryParse(_remindId, CultureInfo.InvariantCulture, out var remindId))
            await _rabbitPublisher.PublishAsync(new RemindMessageNotifyPayload(remindId, payload.MessageId));
    }
}
