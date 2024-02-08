﻿using Discord.Interactions;
using Discord.Net;
using GrillBot.App.Services.Discord;
using GrillBot.Common.Managers;

namespace GrillBot.App.Handlers;

public class InteractionHandler
{
    private InteractionService InteractionService { get; }
    private IServiceProvider Provider { get; }
    private InitManager InitManager { get; }
    private DiscordSocketClient DiscordClient { get; }

    public InteractionHandler(DiscordSocketClient client, IServiceProvider provider, InteractionService interactionService, InitManager initManager)
    {
        Provider = provider;
        InteractionService = interactionService;
        InitManager = initManager;
        DiscordClient = client;

        DiscordClient.InteractionCreated += HandleInteractionAsync;
        InteractionService.SlashCommandExecuted += OnCommandExecutedAsync;
        InteractionService.ContextCommandExecuted += OnCommandExecutedAsync;
        InteractionService.AutocompleteCommandExecuted += OnCommandExecutedAsync;
        InteractionService.ComponentCommandExecuted += OnCommandExecutedAsync;
        InteractionService.ModalCommandExecuted += OnCommandExecutedAsync;
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        if (!InitManager.Get()) return;

        var context = new SocketInteractionContext(DiscordClient, interaction);
        CommandsPerformanceCounter.StartTask(context);

        await InteractionService.ExecuteCommandAsync(context, Provider);
    }

    private static async Task OnCommandExecutedAsync(ICommandInfo command, IInteractionContext context, IResult? result)
    {
        result ??= ExecuteResult.FromSuccess();

        if (result is { IsSuccess: false, Error: { } })
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
                    try
                    {
                        await context.Interaction.RespondAsync(reply, ephemeral: true);
                    }
                    catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.UnknownWebhook)
                    {
                        return;
                    }

                    return;
                }

                if (context.Interaction.HasResponded)
                {
                    try
                    {
                        await context.Interaction.FollowupAsync(reply, ephemeral: true);
                    }
                    catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.UnknownWebhook)
                    {
                        return;
                    }

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
    }
}
