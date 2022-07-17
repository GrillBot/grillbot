using Discord.Interactions;
using GrillBot.App.Services.Guild;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Preconditions.Interactions;

public class RequireGuildEventAttribute : PreconditionAttribute
{
    private string Error { get; }
    private string EventId { get; }

    public RequireGuildEventAttribute(string eventId, string error = null)
    {
        EventId = eventId;
        Error = error;
    }

    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var guildEventsService = services.GetRequiredService<GuildEventsService>();
        var existsEvent = await guildEventsService.ExistsValidGuildEventAsync(context.Guild, EventId);

        return !existsEvent ? PreconditionResult.FromError(GetErrorMessage()) : PreconditionResult.FromSuccess();
    }

    private string GetErrorMessage()
        => string.IsNullOrEmpty(Error) ? "Není platné období pro provedení tohoto příkazu." : Error;
}
