using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
public class MockingModule : InteractionsModuleBase
{
    public MockingModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("mock", "Mocks the specified message")]
    public Task MockAsync(
        [Summary("message", "Message for mocking")]
        string message
    )
    {
        using var command = GetCommand<Actions.Commands.Mock>();
        var result = command.Command.Process(message);

        return SetResponseAsync(result);
    }

    [MessageCommand("Mock")]
    public Task MockAsync(IMessage message)
    {
        using var command = GetCommand<Actions.Commands.Mock>();
        var result = command.Command.Process(message.Content);

        return SetResponseAsync(result);
    }
}
