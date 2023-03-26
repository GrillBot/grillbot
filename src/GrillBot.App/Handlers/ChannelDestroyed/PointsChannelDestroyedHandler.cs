using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.PointsService.Models;
using GrillBot.Database.Enums;
using ChannelInfo = GrillBot.Common.Services.PointsService.Models.ChannelInfo;

namespace GrillBot.App.Handlers.ChannelDestroyed;

public class PointsChannelDestroyedHandler : IChannelDestroyedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    public PointsChannelDestroyedHandler(IPointsServiceClient pointsServiceClient, GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
        PointsServiceClient = pointsServiceClient;
    }

    public async Task ProcessAsync(IChannel channel)
    {
        if (channel is IThreadChannel || channel is not IGuildChannel guildChannel) return;

        var request = await CreateRequestAsync(guildChannel);
        await PointsServiceClient.ProcessSynchronizationAsync(request);
    }

    private async Task<SynchronizationRequest> CreateRequestAsync(IGuildChannel channel)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

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
