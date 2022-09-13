using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
public class MockingModule : InteractionsModuleBase
{
    private MockingService MockingService { get; }

    public MockingModule(MockingService mockingService)
    {
        MockingService = mockingService;
    }

    [SlashCommand("mock", "Mocks the specified message")]
    public Task MockAsync(
        [Summary("message", "Message for mocking")]
        string message
    )
    {
        return SetResponseAsync(content: MockingService.CreateMockingString(message));
    }

    [MessageCommand("Mock")]
    public Task MockAsync(IMessage message)
    {
        return SetResponseAsync(content: MockingService.CreateMockingString(message.Content));
    }
}
