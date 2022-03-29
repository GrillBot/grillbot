using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;

namespace GrillBot.App.Handlers;

[Initializable]
public class InteractionHandler : ServiceBase
{
    private InteractionService InteractionService { get; }
    private IServiceProvider Provider { get; }
    private AuditLogService AuditLogService { get; }

    public InteractionHandler(DiscordSocketClient client, GrillBotContextFactory dbFactory, IServiceProvider provider,
        InteractionService interactionService, DiscordInitializationService initializationService, AuditLogService auditLogService) : base(client, dbFactory, initializationService)
    {
        Provider = provider;
        InteractionService = interactionService;
        AuditLogService = auditLogService;

        DiscordClient.InteractionCreated += HandleInteractionAsync;
        InteractionService.SlashCommandExecuted += OnCommandExecutedAsync;
        InteractionService.ContextCommandExecuted += OnCommandExecutedAsync;
        InteractionService.AutocompleteCommandExecuted += OnCommandExecutedAsync;
        InteractionService.ComponentCommandExecuted += OnCommandExecutedAsync;
        InteractionService.ModalCommandExecuted += OnCommandExecutedAsync;
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        if (!InitializationService.Get()) return;

        var context = new SocketInteractionContext(DiscordClient, interaction);

        if (interaction is SocketSlashCommand || interaction is SocketMessageCommand || interaction is SocketUserCommand)
            await context.Interaction.DeferAsync();

        await InteractionService.ExecuteCommandAsync(context, Provider);
    }

    private async Task OnCommandExecutedAsync(ICommandInfo command, IInteractionContext context, IResult result)
    {
        if (result == null) result = ExecuteResult.FromSuccess();

        if (!result.IsSuccess)
        {
            string reply = "";

            switch (result.Error.Value)
            {
                case InteractionCommandError.UnmetPrecondition:
                case InteractionCommandError.Unsuccessful:
                case InteractionCommandError.ParseFailed:
                case InteractionCommandError.ConvertFailed:
                    reply = result.ErrorReason;
                    break;
                case InteractionCommandError.Exception:
                    var originalMessage = await context.Interaction.GetOriginalResponseAsync();
                    if (originalMessage != null)
                        await originalMessage.AddReactionAsync(Emojis.Nok);
                    break;
            }

            if (!string.IsNullOrEmpty(reply) && context.Interaction is not SocketMessageComponent)
            {
                await context.Interaction.ModifyOriginalResponseAsync(msg => msg.Content = "Command failed");
                var originalMessage = await context.Interaction.GetOriginalResponseAsync();
                await originalMessage.ReplyAsync($"{context.User.Mention} {reply}",
                    allowedMentions: new AllowedMentions(AllowedMentionTypes.Users) { MentionRepliedUser = true });
            }
        }

        if (result.Error != InteractionCommandError.UnknownCommand)
            await AuditLogService.LogExecutedInteractionCommandAsync(command, context, result);
    }
}
