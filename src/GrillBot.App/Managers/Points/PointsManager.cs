﻿using GrillBot.Core.RabbitMQ.V2.Messages;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.PointsService.Models.Channels;
using GrillBot.Core.Services.PointsService.Models.Users;

namespace GrillBot.App.Managers.Points;

public class PointsManager
{
    private readonly PointsSynchronizationManager _synchronizationManager;
    private readonly PointsValidationManager _validationManager;

    private readonly IRabbitPublisher _rabbitPublisher;

    public PointsManager(PointsSynchronizationManager synchronizationManager, PointsValidationManager validationManager, IRabbitPublisher rabbitPublisher)
    {
        _synchronizationManager = synchronizationManager;
        _validationManager = validationManager;
        _rabbitPublisher = rabbitPublisher;
    }

    #region Validations

    public bool CanIncrementPoints(IMessage message, IUser? reactionUser = null) => _validationManager.CanIncrementPoints(message, reactionUser);
    public Task<bool> IsUserAcceptableAsync(IUser user) => _validationManager.IsUserAcceptableAsync(user);

    #endregion

    #region Synchronization

    public Task PushSynchronizationAsync(IGuild guild, params IUser[] users) => PushSynchronizationAsync(guild, users, []);
    public Task PushSynchronizationAsync(IGuild guild, IEnumerable<IUser> users, IEnumerable<IGuildChannel> channels) => _synchronizationManager.PushAsync(guild, users, channels);
    public Task PushSynchronizationAsync(IGuild guild, IEnumerable<UserSyncItem> users, IEnumerable<ChannelSyncItem> channels) => _synchronizationManager.PushAsync(guild, users, channels);
    public Task PushSynchronizationUsersAsync(IEnumerable<IUser> users) => _synchronizationManager.PushUsersAsync(users);

    #endregion

    #region Push

    public Task PushPayloadAsync<TPayload>(TPayload payload) where TPayload : IRabbitMessage
        => _rabbitPublisher.PublishAsync(payload);

    #endregion
}
