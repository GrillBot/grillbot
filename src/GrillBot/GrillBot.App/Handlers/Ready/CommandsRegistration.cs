using Discord.Interactions;
using Discord.Net;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Managers.Logging;

namespace GrillBot.App.Handlers.Ready;

public class CommandsRegistration : IReadyEvent
{
    private InteractionService InteractionService { get; }
    private IDiscordClient DiscordClient { get; }
    private LoggingManager LoggingManager { get; }

    public CommandsRegistration(InteractionService interactionService, IDiscordClient discordClient, LoggingManager loggingManager)
    {
        InteractionService = interactionService;
        DiscordClient = discordClient;
        LoggingManager = loggingManager;
    }

    public async Task ProcessAsync()
    {
        if (InteractionService.Modules.Count == 0) return;

        var guilds = await DiscordClient.GetGuildsAsync();
        foreach (var guild in guilds)
        {
            try
            {
                await InteractionService.RegisterCommandsToGuildAsync(guild.Id);
            }
            catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.MissingOAuth2Scope)
            {
                await LoggingManager.ErrorAsync("Event(Ready)", $"Guild {guild.Name} not have OAuth2 scope for interaction registration.", ex);
            }
        }
    }
}
