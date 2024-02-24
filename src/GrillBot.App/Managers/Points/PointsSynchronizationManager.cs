using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Extensions.RabbitMQ;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Database.Enums;
using PointsModels = GrillBot.Core.Services.PointsService.Models;

namespace GrillBot.App.Managers.Points;

public class PointsSynchronizationManager
{
    private readonly GrillBotDatabaseBuilder _databaseBuilder;
    private readonly IRabbitMQPublisher _rabbitPublisher;

    public PointsSynchronizationManager(GrillBotDatabaseBuilder databaseBuilder, IRabbitMQPublisher rabbitPublisher)
    {
        _databaseBuilder = databaseBuilder;
        _rabbitPublisher = rabbitPublisher;
    }

    public async Task PushAsync(IGuild guild, IEnumerable<IUser> users, IEnumerable<IGuildChannel> channels)
    {
        var channelInfos = new List<PointsModels.ChannelInfo>();
        var userInfos = new List<PointsModels.Users.UserInfo>();

        await using var repository = _databaseBuilder.CreateRepository();

        foreach (var user in users)
        {
            var entity = await repository.User.FindUserAsync(user, true);
            if (entity is null) continue;

            userInfos.Add(new PointsModels.Users.UserInfo
            {
                PointsDisabled = entity.HaveFlags(UserFlags.PointsDisabled),
                Id = user.Id.ToString(),
                IsUser = user.IsUser()
            });
        }

        foreach (var channelId in channels.Select(o => o.Id))
        {
            var entity = await repository.Channel.FindChannelByIdAsync(channelId, guild.Id, true, includeDeleted: true);
            if (entity is null) continue;

            channelInfos.Add(new PointsModels.ChannelInfo
            {
                Id = channelId.ToString(),
                PointsDisabled = entity.HasFlag(ChannelFlag.PointsDeactivated),
                IsDeleted = entity.HasFlag(ChannelFlag.Deleted)
            });
        }

        var payload = new PointsModels.Events.SynchronizationPayload(guild.Id.ToString(), channelInfos, userInfos);
        await _rabbitPublisher.PushAsync(payload);
    }
}
