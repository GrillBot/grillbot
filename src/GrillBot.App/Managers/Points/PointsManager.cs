using GrillBot.Common.Extensions.RabbitMQ;
using GrillBot.Core.RabbitMQ.Publisher;

namespace GrillBot.App.Managers.Points;

public class PointsManager
{
    private readonly PointsSynchronizationManager _synchronizationManager;
    private readonly PointsValidationManager _validationManager;

    private readonly IRabbitMQPublisher _rabbitPublisher;

    public PointsManager(PointsSynchronizationManager synchronizationManager, PointsValidationManager validationManager, IRabbitMQPublisher rabbitPublisher)
    {
        _synchronizationManager = synchronizationManager;
        _validationManager = validationManager;
        _rabbitPublisher = rabbitPublisher;
    }

    #region Validations

    public Task<bool> CanIncrementPointsAsync(IMessage message, IUser? reactionUser = null) => _validationManager.CanIncrementPointsAsync(message, reactionUser);
    public Task<bool> IsUserAcceptableAsync(IUser user) => _validationManager.IsUserAcceptableAsync(user);

    #endregion

    #region Synchronization

    public Task PushSynchronizationAsync(IGuild guild, params IUser[] users) => PushSynchronizationAsync(guild, users, Enumerable.Empty<IGuildChannel>());
    public Task PushSynchronizationAsync(IGuildChannel channel) => PushSynchronizationAsync(channel.Guild, Enumerable.Empty<IUser>(), new[] { channel });
    public Task PushSynchronizationAsync(IGuild guild, params IGuildChannel[] channels) => PushSynchronizationAsync(guild, Enumerable.Empty<IUser>(), channels);
    public Task PushSynchronizationAsync(IGuild guild, IEnumerable<IUser> users, IEnumerable<IGuildChannel> channels) => _synchronizationManager.PushAsync(guild, users, channels);

    #endregion

    #region Push

    public Task PushPayloadAsync<TPayload>(TPayload payload) => _rabbitPublisher.PushAsync(payload);

    #endregion
}
