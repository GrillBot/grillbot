using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Common.Managers.Cooldown;
using GrillBot.Common.Managers.Events.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.App.Handlers.InteractionCommandExecuted;

public class InteractionFailedCommandHandler : IInteractionCommandExecutedEvent
{
    private CooldownManager CooldownManager { get; }

    public InteractionFailedCommandHandler(CooldownManager cooldownManager)
    {
        CooldownManager = cooldownManager;
    }

    public Task ProcessAsync(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess || !TryGetAttribute(commandInfo, out var attribute))
            return Task.CompletedTask;

        var id = CooldownCheckAttribute.PickId(context, attribute.Type);
        var bannedUntil = DateTime.Now.AddSeconds(attribute.Seconds);
        CooldownManager.DecreaseCooldown(id, attribute.Type, bannedUntil);

        return Task.CompletedTask;
    }

    private static bool TryGetAttribute(ICommandInfo commandInfo, [MaybeNullWhen(false)] out CooldownCheckAttribute attribute)
    {
        var attr = commandInfo.Preconditions.OfType<CooldownCheckAttribute>().FirstOrDefault();
        attr ??= commandInfo.Attributes.OfType<CooldownCheckAttribute>().FirstOrDefault();

        attribute = attr;
        return attr is not null;
    }
}
