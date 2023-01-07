using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IMessageReceivedEvent
{
    Task ProcessAsync(IMessage message);
}
