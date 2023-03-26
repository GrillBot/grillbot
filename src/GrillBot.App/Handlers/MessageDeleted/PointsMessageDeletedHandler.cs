using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.PointsService;

namespace GrillBot.App.Handlers.MessageDeleted;

public class PointsMessageDeletedHandler : IMessageDeletedEvent
{
    private IPointsServiceClient PointsServiceClient { get; }

    private IGuildChannel? Channel { get; set; }

    public PointsMessageDeletedHandler(IPointsServiceClient pointsServiceClient)
    {
        PointsServiceClient = pointsServiceClient;
    }

    public async Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        Init(cachedChannel);
        if (Channel is null) return;

        await PointsServiceClient.DeleteTransactionAsync(Channel.GuildId.ToString(), cachedMessage.Id.ToString());
    }

    private void Init(Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not IGuildChannel guildChannel) return;
        Channel = guildChannel;
    }
}
