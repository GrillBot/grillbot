using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.MessageReceived;

public class ChannelPinMessageReceivedHandler : IMessageReceivedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public ChannelPinMessageReceivedHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IMessage message)
    {
        if (!CanProcess(message) || message.Channel is IVoiceChannel || message.Channel is not ITextChannel channel)
            return;

        var pins = await channel.GetPinnedMessagesAsync();

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(channel.Guild);

        var dbChannel = await repository.Channel.GetOrCreateChannelAsync(channel);
        dbChannel.PinCount = pins.Count;
        await repository.CommitAsync();
    }

    private static bool CanProcess(IMessage message)
        => message.Source is MessageSource.System && message.Type is MessageType.ChannelPinnedMessage;
}
