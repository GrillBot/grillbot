using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Common.Managers.Cooldown;
using GrillBot.Common.Managers.Events.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.App.Handlers.InteractionCommandExecuted;

public class InteractionFailedCommandHandler(CooldownManager _cooldownManager) : IInteractionCommandExecutedEvent
{
    public async Task ProcessAsync(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess || !TryGetAttribute(commandInfo, out var attribute))
            return;

        var id = CooldownCheckAttribute.PickId(context, attribute.Type);
        var bannedUntil = DateTime.Now.AddSeconds(attribute.Seconds);

        if (result.Error == InteractionCommandError.UnmetPrecondition)
        {
            var remainingCooldown = await _cooldownManager.GetRemainingCooldownAsync(id, attribute.Type);
            if (remainingCooldown?.TotalSeconds >= 1)
                return;
        }

        await _cooldownManager.DecreaseCooldownAsync(id, attribute.Type, bannedUntil);
    }

    private static bool TryGetAttribute(ICommandInfo commandInfo, [MaybeNullWhen(false)] out CooldownCheckAttribute attribute)
    {
        var attr = commandInfo.Preconditions.OfType<CooldownCheckAttribute>().FirstOrDefault();
        attr ??= commandInfo.Attributes.OfType<CooldownCheckAttribute>().FirstOrDefault();

        attribute = attr;
        return attr is not null;
    }
}
