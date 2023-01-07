using Discord;
using Discord.Interactions;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IInteractionCommandExecutedEvent
{
    Task ProcessAsync(ICommandInfo commandInfo, IInteractionContext context, IResult result);
}
