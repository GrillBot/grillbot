using Discord.Interactions;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Cooldown;
using GrillBot.Common.Managers.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Preconditions.Interactions;

public class CooldownCheckAttribute : PreconditionAttribute
{
    private CooldownType Type { get; }
    private int Seconds { get; }
    private int MaxAllowedCount { get; }

    public CooldownCheckAttribute(CooldownType type, int seconds, int maxAllowedCount)
    {
        Type = type;
        Seconds = seconds;
        MaxAllowedCount = maxAllowedCount;
    }

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var cooldownManager = services.GetRequiredService<CooldownManager>();

        var id = PickId(context, Type);
        if (cooldownManager.IsCooldown(id, Type, out var remains))
        {
            var texts = services.GetRequiredService<ITextsManager>();
            var remainsValue = remains!.Value.Humanize(culture: texts.GetCulture(context.Interaction.UserLocale), precision: int.MaxValue, minUnit: TimeUnit.Minute);
            return Task.FromResult(PreconditionResult.FromError(texts["CooldownEnabled", context.Interaction.UserLocale].FormatWith(remainsValue)));
        }

        var bannedUntil = DateTime.Now.AddSeconds(Seconds);
        cooldownManager.SetCooldown(id, Type, MaxAllowedCount, bannedUntil);
        return Task.FromResult(PreconditionResult.FromSuccess());
    }

    private static string PickId(IInteractionContext context, CooldownType type)
    {
        return type switch
        {
            CooldownType.Channel => context.Channel.Id.ToString(),
            CooldownType.Guild => context.Guild.Id.ToString(),
            _ => context.User.Id.ToString()
        };
    }
}
