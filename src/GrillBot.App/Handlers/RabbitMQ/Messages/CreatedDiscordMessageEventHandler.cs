using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.RabbitMQ.V2.Consumer;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using Emote.Models.Events.Suggestions;
using GrillBot.Models.Events.Messages;
using RemindService.Models.Events;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ.Messages;

public class CreatedDiscordMessageEventHandler(
    ILoggerFactory loggerFactory,
    IRabbitPublisher _rabbitPublisher
) : RabbitMessageHandlerBase<CreatedDiscordMessagePayload>(loggerFactory)
{
    protected override async Task<RabbitConsumptionResult> HandleInternalAsync(
        CreatedDiscordMessagePayload message,
        ICurrentUserProvider currentUser,
        Dictionary<string, string> headers,
        CancellationToken cancellationToken = default
    )
    {
        switch (message.ServiceId)
        {
            case "Remind":
                await ProcessRemindServiceMessageAsync(message, cancellationToken);
                break;
            case "Emote":
                await ProcessEmoteServiceMessageAsync(message, cancellationToken);
                break;
        }

        return RabbitConsumptionResult.Success;
    }

    private Task ProcessRemindServiceMessageAsync(CreatedDiscordMessagePayload payload, CancellationToken cancellationToken = default)
    {
        if (payload.ServiceData.TryGetValue("RemindId", out var _remindId) && long.TryParse(_remindId, CultureInfo.InvariantCulture, out var remindId))
            return _rabbitPublisher.PublishAsync(new RemindMessageNotifyPayload(remindId, payload.MessageId), cancellationToken: cancellationToken);
        return Task.CompletedTask;
    }

    private Task ProcessEmoteServiceMessageAsync(CreatedDiscordMessagePayload payload, CancellationToken cancellationToken = default)
    {
        if (
            !payload.ServiceData.TryGetValue("SuggestionId", out var _suggestionId) ||
            !Guid.TryParse(_suggestionId, out var suggestionId) ||
            !payload.ServiceData.TryGetValue("MessageType", out var messageType)
        )
        {
            return Task.CompletedTask;
        }

        return messageType switch
        {
            "VoteMessage" => _rabbitPublisher.PublishAsync(new EmoteSuggestionVoteMessageCreatedPayload(suggestionId, payload.MessageId.ToUlong()), cancellationToken: cancellationToken),
            "SuggestionMessage" => _rabbitPublisher.PublishAsync(new EmoteSuggestionMessageCreatedPayload(suggestionId, payload.MessageId.ToUlong()), cancellationToken: cancellationToken),
            _ => Task.CompletedTask,
        };
    }
}
