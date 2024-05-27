using GrillBot.Core.RabbitMQ.Consumer;
using GrillBot.Core.Services.GrillBot.Models.Events.Messages;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ;

public class CreatedDiscordMessageEventHandler : BaseRabbitMQHandler<CreatedDiscordMessagePayload>
{
    public override string QueueName => new CreatedDiscordMessagePayload().QueueName;

    public CreatedDiscordMessageEventHandler(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    protected override Task HandleInternalAsync(CreatedDiscordMessagePayload payload, Dictionary<string, string> headers)
    {
        return Task.CompletedTask;
    }
}
