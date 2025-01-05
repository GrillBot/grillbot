using Discord.Interactions;
using GrillBot.Common.Managers.Cooldown;
using GrillBot.Common.Managers.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Preconditions.Interactions;

public class CooldownCheckAttribute : PreconditionAttribute
{
    public CooldownType Type { get; }
    public int Seconds { get; }
    private int MaxAllowedCount { get; }

    public CooldownCheckAttribute(CooldownType type, int seconds, int maxAllowedCount)
    {
        Type = type;
        Seconds = seconds;
        MaxAllowedCount = maxAllowedCount;
    }

    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var cooldownManager = services.GetRequiredService<CooldownManager>();

        var id = PickId(context, Type);
        var remainingCooldown = await cooldownManager.GetRemainingCooldownAsync(id, Type);

        if (remainingCooldown is not null)
        {
            var texts = services.GetRequiredService<ITextsManager>();
            var culture = texts.GetCulture(context.Interaction.UserLocale);
            var remainsValue = remainingCooldown.Value.Humanize(precision: int.MaxValue, culture: culture, minUnit: TimeUnit.Minute);
            var result = texts["CooldownEnabled", context.Interaction.UserLocale].FormatWith(remainsValue);

            return PreconditionResult.FromError(result);
        }

        var bannedUntil = DateTime.Now.AddSeconds(Seconds);
        await cooldownManager.SetCooldownAsync(id, Type, MaxAllowedCount, bannedUntil);
        return PreconditionResult.FromSuccess();
    }

    public static string PickId(IInteractionContext context, CooldownType type)
    {
        return type switch
        {
            CooldownType.Channel => context.Channel.Id.ToString(),
            CooldownType.Guild => context.Guild.Id.ToString(),
            _ => context.User.Id.ToString()
        };
    }
}
