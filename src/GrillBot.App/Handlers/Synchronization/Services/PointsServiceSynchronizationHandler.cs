using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.PointsService.Models;
using GrillBot.Database.Enums;
using ChannelInfo = GrillBot.Common.Services.PointsService.Models.ChannelInfo;

namespace GrillBot.App.Handlers.Synchronization.Services;

public class PointsServiceSynchronizationHandler : BaseSynchronizationHandler<IPointsServiceClient>, IChannelDestroyedEvent, IThreadDeletedEvent
{
    public PointsServiceSynchronizationHandler(IPointsServiceClient serviceClient, GrillBotDatabaseBuilder databaseBuilder) : base(serviceClient, databaseBuilder)
    {
    }

    // ChannelDestroyed
    public async Task ProcessAsync(IChannel channel)
    {
        if (channel is IThreadChannel || channel is not IGuildChannel guildChannel)
            return;

        var request = await CreateSynchronizationRequestAsync(guildChannel);
        await ServiceClient.ProcessSynchronizationAsync(request);
    }

    // ThreadDeleted
    public async Task ProcessAsync(IThreadChannel? cachedThread, ulong threadId)
    {
        if (cachedThread is null) return;

        var request = await CreateSynchronizationRequestAsync(cachedThread);
        await ServiceClient.ProcessSynchronizationAsync(request);
    }

    private async Task<SynchronizationRequest> CreateSynchronizationRequestAsync(IGuildChannel channel)
    {
        await using var repository = CreateRepository();

        return new SynchronizationRequest
        {
            Channels = new List<ChannelInfo>
            {
                new()
                {
                    Id = channel.Id.ToString(),
                    IsDeleted = true,
                    PointsDisabled = await repository.Channel.HaveChannelFlagsAsync(channel, ChannelFlag.PointsDeactivated)
                }
            },
            GuildId = channel.GuildId.ToString()
        };
    }
}
