using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.PointsService.Models;
using GrillBot.Database.Enums;
using ChannelInfo = GrillBot.Common.Services.PointsService.Models.ChannelInfo;

namespace GrillBot.App.Handlers.ThreadDeleted;

public class PointsThreadDeletedHandler : IThreadDeletedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    public PointsThreadDeletedHandler(IPointsServiceClient pointsServiceClient, GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
        PointsServiceClient = pointsServiceClient;
    }

    public async Task ProcessAsync(IThreadChannel? cachedThread, ulong threadId)
    {
        if (cachedThread is null) return;

        var request = await CreateRequestAsync(cachedThread);
        await PointsServiceClient.ProcessSynchronizationAsync(request);
    }

    private async Task<SynchronizationRequest> CreateRequestAsync(IGuildChannel thread)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        return new SynchronizationRequest
        {
            Channels = new List<ChannelInfo>
            {
                new()
                {
                    Id = thread.Id.ToString(),
                    IsDeleted = true,
                    PointsDisabled = await repository.Channel.HaveChannelFlagsAsync(thread, ChannelFlag.PointsDeactivated)
                }
            },
            GuildId = thread.GuildId.ToString()
        };
    }
}
