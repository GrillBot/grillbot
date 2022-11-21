using Discord.Interactions;
using Discord.Net;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.Common.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Handlers;

[Initializable]
public class InteractionHandler
{
    private InteractionService InteractionService { get; }
    private IServiceProvider Provider { get; }
    private AuditLogService AuditLogService { get; }
    private InitManager InitManager { get; }
    private DiscordSocketClient DiscordClient { get; }

    public InteractionHandler(DiscordSocketClient client, IServiceProvider provider, InteractionService interactionService, InitManager initManager, AuditLogService auditLogService)
    {
        Provider = provider;
        InteractionService = interactionService;
        AuditLogService = auditLogService;
        InitManager = initManager;
        DiscordClient = client;

        DiscordClient.InteractionCreated += HandleInteractionAsync;
        InteractionService.SlashCommandExecuted += OnCommandExecutedAsync;
        InteractionService.ContextCommandExecuted += OnCommandExecutedAsync;
        InteractionService.AutocompleteCommandExecuted += OnCommandExecutedAsync;
        InteractionService.ComponentCommandExecuted += OnCommandExecutedAsync;
        InteractionService.ModalCommandExecuted += OnCommandExecutedAsync;
        DiscordClient.MessageReceived += HandleUnsuccessfulAttempt;
    }

    private async Task HandleUnsuccessfulAttempt(SocketMessage message)
    {
        if (!InitManager.Get() || !message.Content.StartsWith('/') || message.Channel is IDMChannel) return;

        using var scope = Provider.CreateScope();
        var action = scope.ServiceProvider.GetRequiredService<Actions.Commands.UnsuccessCommandAttempt>();
        await action.ProcessAsync(message);
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        if (!InitManager.Get()) return;

        var context = new SocketInteractionContext(DiscordClient, interaction);
        CommandsPerformanceCounter.StartTask(context);

        await InteractionService.ExecuteCommandAsync(context, Provider);
    }

    private async Task OnCommandExecutedAsync(ICommandInfo command, IInteractionContext context, IResult result)
    {
        result ??= ExecuteResult.FromSuccess();

        if (!result.IsSuccess && result.Error.HasValue)
        {
            var reply = "";

            switch (result.Error.Value)
            {
                case InteractionCommandError.UnmetPrecondition:
                case InteractionCommandError.Unsuccessful:
                case InteractionCommandError.ParseFailed:
                case InteractionCommandError.ConvertFailed:
                    reply = result.ErrorReason;
                    break;
                case InteractionCommandError.Exception:
                    reply = $"Error: {result.ErrorReason}";
                    break;
                case InteractionCommandError.UnknownCommand:
                case InteractionCommandError.BadArgs:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(null, nameof(result.Error));
            }

            if (!string.IsNullOrEmpty(reply))
            {
                if (!context.Interaction.HasResponded && (new DateTimeOffset(DateTime.UtcNow) - context.Interaction.CreatedAt).TotalSeconds <= 3.0)
                {
                    await context.Interaction.RespondAsync(reply, ephemeral: true);
                    return;
                }

                try
                {
                    await context.User.SendMessageAsync(reply);
                }
                catch (HttpException ex) when (ex.DiscordCode != DiscordErrorCode.CannotSendMessageToUser)
                {
                    throw;
                }
            }
        }

        if (result.Error != InteractionCommandError.UnknownCommand)
        {
            var duration = CommandsPerformanceCounter.TaskFinished(context);
            await AuditLogService.LogExecutedInteractionCommandAsync(command, context, result, duration);
        }
        else
        {
            if (CommandsPerformanceCounter.TaskExists(context))
                CommandsPerformanceCounter.TaskFinished(context);
        }
    }
}
