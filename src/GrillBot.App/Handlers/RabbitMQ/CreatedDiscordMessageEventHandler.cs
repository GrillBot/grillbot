using GrillBot.Core.RabbitMQ.Consumer;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.GrillBot.Models.Events.Messages;
using GrillBot.Core.Services.RemindService.Models.Events;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ;

public class CreatedDiscordMessageEventHandler : BaseRabbitMQHandler<CreatedDiscordMessagePayload>
{
    public override string QueueName => new CreatedDiscordMessagePayload().QueueName;

    private readonly IRabbitMQPublisher _rabbitPublisher;

    public CreatedDiscordMessageEventHandler(ILoggerFactory loggerFactory, IRabbitMQPublisher rabbitPublisher) : base(loggerFactory)
    {
        _rabbitPublisher = rabbitPublisher;
    }

    protected override Task HandleInternalAsync(CreatedDiscordMessagePayload payload, Dictionary<string, string> headers)
    {
        return payload.ServiceId switch
        {
            "Remind" => ProcessRemindServiceMessageAsync(payload),
            _ => Task.CompletedTask,
        };
    }

    private async Task ProcessRemindServiceMessageAsync(CreatedDiscordMessagePayload payload)
    {
        if (payload.ServiceData.TryGetValue("RemindId", out var _remindId) && long.TryParse(_remindId, CultureInfo.InvariantCulture, out var remindId))
            await _rabbitPublisher.PublishAsync(new RemindMessageNotifyPayload(remindId, payload.MessageId));
    }
}
