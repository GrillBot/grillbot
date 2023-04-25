using GrillBot.App.Helpers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.PointsService.Models;
using GrillBot.Database.Enums;
using ChannelInfo = GrillBot.Common.Services.PointsService.Models.ChannelInfo;

namespace GrillBot.App.Handlers.Synchronization.Services;

public class PointsServiceSynchronizationHandler : BaseSynchronizationHandler<IPointsServiceClient>, IUserUpdatedEvent, IChannelDestroyedEvent, IThreadDeletedEvent
{
    private PointsHelper PointsHelper { get; }
    private IDiscordClient DiscordClient { get; }

    public PointsServiceSynchronizationHandler(IPointsServiceClient serviceClient, PointsHelper pointsHelper, GrillBotDatabaseBuilder databaseBuilder,
        IDiscordClient discordClient) : base(serviceClient, databaseBuilder)
    {
        PointsHelper = pointsHelper;
        DiscordClient = discordClient;
    }

    public async Task ProcessAsync(IUser before, IUser after)
    {
        if (!after.IsUser())
            return;

        var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(after.Id);
        foreach (var guild in mutualGuilds)
            await PointsHelper.SyncDataWithServiceAsync(guild, new[] { after }, Enumerable.Empty<IGuildChannel>());
    }

    public async Task ProcessAsync(IChannel channel)
    {
        if (channel is IThreadChannel || channel is not IGuildChannel guildChannel)
            return;

        var request = await CreateSynchronizationRequestAsync(guildChannel);
        await ServiceClient.ProcessSynchronizationAsync(request);
    }

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
        ;
    }
}
