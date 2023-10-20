using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Preconditions.Interactions;

public class RequireValidEmoteSuggestionsAttribute : PreconditionAttribute
{
    private string? Error { get; }

    public RequireValidEmoteSuggestionsAttribute(string? error = null)
    {
        Error = error;
    }

    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var databaseBuilder = services.GetRequiredService<GrillBotDatabaseBuilder>();

        await using var repository = databaseBuilder.CreateRepository();
        var guild = await repository.Guild.FindGuildAsync(context.Guild, true);

        var now = DateTime.Now;
        var from = guild?.EmoteSuggestionsFrom.GetValueOrDefault();
        var to = guild?.EmoteSuggestionsTo.GetValueOrDefault();
        var isValidEvent = now >= from && now < to;
        return !isValidEvent ? PreconditionResult.FromError(GetErrorMessage()) : PreconditionResult.FromSuccess();
    }

    private string? GetErrorMessage()
        => string.IsNullOrEmpty(Error) ? "Není platné období pro provedení tohoto příkazu." : Error;
}
