using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.MessageService.Models.Events;
using GrillBot.Core.Services.MessageService.Models.Events.Channels;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public class MessageOrchestrationHandler(
    IRabbitPublisher _publisher
) : IMessageReceivedEvent, IThreadDeletedEvent, IChannelDestroyedEvent
{
    // MessageReceived
    public Task ProcessAsync(IMessage message)
    {
        var payload = MessageReceivedPayload.Create(message);
        return payload is not null ? _publisher.PublishAsync(payload) : Task.CompletedTask;
    }

    // ThreadDeleted
    public Task ProcessAsync(IThreadChannel? cachedThread, ulong threadId)
    {
        if (cachedThread is null)
            return Task.CompletedTask;

        var syncItem = ChannelSynchronizationItem.FromChannel(cachedThread);
        syncItem.IsDeleted = true;

        return _publisher.PublishAsync(new SynchronizationPayload([syncItem]));
    }

    // ChannelDestroyed
    public Task ProcessAsync(IChannel channel)
    {
        if (channel is not IGuildChannel guildChannel)
            return Task.CompletedTask;

        var syncItem = ChannelSynchronizationItem.FromChannel(guildChannel);
        syncItem.IsDeleted = true;

        return _publisher.PublishAsync(new SynchronizationPayload([syncItem]));
    }
}
