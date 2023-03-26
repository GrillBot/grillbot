using GrillBot.App.Helpers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.UserUpdated;

public class SyncPointsServiceUserUpdatedHandler : IUserUpdatedEvent
{
    private PointsHelper PointsHelper { get; }

    public SyncPointsServiceUserUpdatedHandler(PointsHelper helper)
    {
        PointsHelper = helper;
    }

    public async Task ProcessAsync(IUser before, IUser after)
    {
        if (after is IGuildUser guildUser && before.IsUser() != after.IsUser())
            await PointsHelper.SyncDataWithServiceAsync(guildUser.Guild, new[] { after }, Enumerable.Empty<IGuildChannel>());
    }
}
