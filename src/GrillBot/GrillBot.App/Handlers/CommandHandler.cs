using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Services.AuditLog;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Handlers
{
    public class CommandHandler : ServiceBase
    {
        private CommandService CommandService { get; }
        private IServiceProvider Provider { get; }
        private IConfiguration Configuration { get; }
        private AuditLogService AuditLogService { get; }

        public CommandHandler(DiscordSocketClient client, CommandService commandService, IServiceProvider provider, IConfiguration configuration,
            AuditLogService auditLogService) : base(client)
        {
            CommandService = commandService;
            Provider = provider;
            Configuration = configuration;
            AuditLogService = auditLogService;

            CommandService.CommandExecuted += OnCommandExecutedAsync;
            DiscordClient.MessageReceived += OnCommandTriggerTryAsync;
        }

        private async Task OnCommandTriggerTryAsync(SocketMessage message)
        {
            if (DiscordClient.Status != UserStatus.Online) return;
            if (!message.TryLoadMessage(out SocketUserMessage userMessage)) return;
            var context = new SocketCommandContext(DiscordClient, userMessage);

            int argumentPosition = 0;
            var prefix = Configuration.GetValue<string>("Discord:Commands:Prefix");
            if (userMessage.IsCommand(ref argumentPosition, DiscordClient.CurrentUser, prefix))
                await CommandService.ExecuteAsync(context, argumentPosition, Provider);
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // Null is success, because some modules returns null after success and library always returns ExecuteResult.
            if (result == null) result = ExecuteResult.FromSuccess();

            if (!result.IsSuccess && result.Error != null)
            {
                string reply = "";

                switch (result.Error.Value)
                {
                    case CommandError.Unsuccessful when result is CommandRedirectResult crr && !string.IsNullOrEmpty(crr.NewCommand):
                        await CommandService.ExecuteAsync(context, crr.NewCommand, Provider);
                        break;

                    case CommandError.ObjectNotFound when result is ParseResult parseResult && typeof(IUser).IsAssignableFrom(parseResult.ErrorParameter.Type):
                        reply = "Bohužel jsem nenalezl uživatele, kterého jsi zadal/a.";
                        break;

                    case CommandError.UnmetPrecondition:
                    case CommandError.Unsuccessful:
                    case CommandError.ParseFailed:
                    case CommandError.ObjectNotFound:
                        reply = result.ErrorReason;
                        break;

                    case CommandError.BadArgCount:
                        await CommandService.ExecuteAsync(context, $"help {context.Message.Content[1..]}", Provider);
                        break;

                    case CommandError.Exception:
                        await context.Message.AddReactionAsync(new Emoji("❌"));
                        break;
                }

                // Reply to command message with mentioning user
                if (!string.IsNullOrEmpty(reply))
                    await context.Message.ReplyAsync(reply, allowedMentions: new AllowedMentions { MentionRepliedUser = true });
            }

            if (result.Error != CommandError.UnknownCommand)
                await AuditLogService.LogExecutedCommandAsync(command.Value, context, result);
        }
    }
}
