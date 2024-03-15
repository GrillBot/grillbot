using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.Synchronization.Database;

public class MessageSynchronizationHandler : BaseSynchronizationHandler, IMessageUpdatedEvent
{
    private IMessageCacheManager MessageCache { get; }

    public MessageSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder, IMessageCacheManager messageCache) : base(databaseBuilder)
    {
        MessageCache = messageCache;
    }

    public async Task ProcessAsync(Cacheable<IMessage, ulong> before, IMessage after, IMessageChannel channel)
    {
        await ProcessPinStateChangeAsync(before, after, channel);
    }

    private async Task ProcessPinStateChangeAsync(Cacheable<IMessage, ulong> before, IMessage after, IMessageChannel channel)
    {
        if (channel is IVoiceChannel || channel is not ITextChannel textChannel)
            return;

        var oldMessage = before.HasValue ? before.Value : null;
        oldMessage ??= await MessageCache.GetAsync(before.Id, null);
        if (oldMessage is null || oldMessage.IsPinned == after.IsPinned)
            return;

        await using var repository = CreateRepository();

        var dbChannel = await repository.Channel.FindChannelByIdAsync(textChannel.Id, textChannel.GuildId);
        if (dbChannel is null)
            return;

        var pins = await textChannel.GetPinnedMessagesAsync();
        dbChannel.PinCount = pins.Count;

        await repository.CommitAsync();
    }
}
