using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.PointsService.Models.Channels;
using GrillBot.Core.Services.PointsService.Models.Users;
using PointsModels = GrillBot.Core.Services.PointsService.Models;

namespace GrillBot.App.Managers.Points;

public class PointsSynchronizationManager
{
    private readonly IRabbitMQPublisher _rabbitPublisher;

    public PointsSynchronizationManager(IRabbitMQPublisher rabbitPublisher)
    {
        _rabbitPublisher = rabbitPublisher;
    }

    public Task PushAsync(IGuild guild, IEnumerable<IUser> users, IEnumerable<IGuildChannel> channels)
    {
        var channelList = channels.Select(o => new ChannelSyncItem { Id = o.Id.ToString() });

        var userList = users.Select(o => new UserSyncItem
        {
            Id = o.Id.ToString(),
            IsUser = o.IsUser()
        });

        return PushAsync(guild, userList, channelList);
    }

    public async Task PushAsync(IGuild guild, IEnumerable<UserSyncItem> users, IEnumerable<ChannelSyncItem> channels)
    {
        var payload = new PointsModels.Events.SynchronizationPayload(guild.Id.ToString(), channels.ToList(), users.ToList());
        await _rabbitPublisher.PublishAsync(payload, new());
    }
}
