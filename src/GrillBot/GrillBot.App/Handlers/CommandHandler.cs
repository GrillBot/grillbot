using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Handlers
{
    public class CommandHandler : Handler
    {
        private CommandService CommandService { get; }
        private IServiceProvider Provider { get; }

        public CommandHandler(DiscordSocketClient client, CommandService commandService, IServiceProvider provider) : base(client)
        {
            CommandService = commandService;
            Provider = provider;

            CommandService.CommandExecuted += OnCommandExecutedAsync;
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

                    case CommandError.UnmetPrecondition:
                    case CommandError.Unsuccessful:
                    case CommandError.ParseFailed:
                        reply = result.ErrorReason;
                        break;

                    case CommandError.ObjectNotFound when result is ParseResult parseResult && typeof(IUser).IsAssignableFrom(parseResult.ErrorParameter.Type):
                        reply = "Bohužel jsem nenalezl uživatele, kterého jsi zadal/a.";
                        break;

                    case CommandError.BadArgCount:
                        await CommandService.ExecuteAsync(context, $"help {context.Message.Content[1..]}", Provider);
                        break;

                    case CommandError.Exception:
                        await context.Message.AddReactionAsync(new Emoji("❌"));
                        break;
                }

                // Reply to command message without mentioning any user
                if (!string.IsNullOrEmpty(reply))
                    await context.Message.ReplyAsync(reply, allowedMentions: new AllowedMentions { MentionRepliedUser = false });
            }
        }
    }
}
