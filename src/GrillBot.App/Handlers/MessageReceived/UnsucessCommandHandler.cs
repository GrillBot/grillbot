using Discord.Commands;
using GrillBot.App.Actions.Commands;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.MessageReceived;

public class UnsucessCommandHandler : IMessageReceivedEvent
{
    private IConfiguration Configuration { get; }
    private CommandService CommandService { get; }
    private IDiscordClient DiscordClient { get; }
    private IServiceProvider ServiceProvider { get; }
    private UnsuccessCommandAttempt UnsuccessCommandAttempt { get; }

    public UnsucessCommandHandler(IConfiguration configuration, CommandService commandService, IDiscordClient discordClient, IServiceProvider serviceProvider,
        UnsuccessCommandAttempt unsuccessCommandAttempt)
    {
        Configuration = configuration;
        CommandService = commandService;
        DiscordClient = discordClient;
        ServiceProvider = serviceProvider;
        UnsuccessCommandAttempt = unsuccessCommandAttempt;
    }

    public async Task ProcessAsync(IMessage message)
    {
        if (!message.TryLoadMessage(out var userMessage) || userMessage == null || message.Channel is IDMChannel) return;

        var prefix = Configuration.GetValue<string>("Discord:Commands:Prefix");
        if (userMessage.Content.StartsWith(prefix))
            await HandleDeprecatedCommandAsync(userMessage, prefix);
        else if (userMessage.Content.StartsWith('/'))
            await UnsuccessCommandAttempt.ProcessAsync(message);
    }

    private async Task HandleDeprecatedCommandAsync(IUserMessage message, string prefix)
    {
        var argPos = 0;
        if (!message.IsCommand(ref argPos, DiscordClient.CurrentUser, prefix)) return;

        var context = new SocketCommandContext((DiscordSocketClient)DiscordClient, (SocketUserMessage)message);
        var result = await CommandService.ExecuteAsync(context, argPos, ServiceProvider);
        if (result.IsSuccess || result.Error == null) return;

        switch (result.Error)
        {
            case CommandError.UnmetPrecondition or CommandError.Unsuccessful or CommandError.ParseFailed or CommandError.ObjectNotFound:
                await context.Message.ReplyAsync(result.ErrorReason, allowedMentions: new AllowedMentions { MentionRepliedUser = true });
                break;
            case CommandError.Exception:
                await context.Message.AddReactionAsync(new Emoji("❌"));
                break;
        }
    }
}
