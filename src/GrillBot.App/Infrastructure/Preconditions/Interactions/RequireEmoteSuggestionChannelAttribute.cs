using Discord.Interactions;
using GrillBot.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Preconditions.Interactions;

public class RequireEmoteSuggestionChannelAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        if (context.Guild == null)
            return PreconditionResult.FromSuccess();

        var databaseBuilder = services.GetRequiredService<GrillBotDatabaseBuilder>();
        await using var repository = databaseBuilder.CreateRepository();

        var guild = await repository.Guild.FindGuildAsync(context.Guild, true);
        if (guild == null)
            return PreconditionResult.FromSuccess();

        if (string.IsNullOrEmpty(guild.EmoteSuggestionChannelId))
            return PreconditionResult.FromError("Tento příkaz nelze provést, protože není nastaven kanál pro návrhy emotů.");

        return await context.Guild.GetTextChannelAsync(guild.EmoteSuggestionChannelId.ToUlong()) == null
            ? PreconditionResult.FromError("Nepodařilo se najít kanál pro návrhy emotů.")
            : PreconditionResult.FromSuccess();
    }
}
