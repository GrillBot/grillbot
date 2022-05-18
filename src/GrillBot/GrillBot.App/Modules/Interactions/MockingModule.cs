using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
public class MockingModule : Infrastructure.InteractionsModuleBase
{
    private MockingService MockingService { get; }

    public MockingModule(MockingService mockingService)
    {
        MockingService = mockingService;
    }

    [SlashCommand("mock", "Mockuje zadanou zprávu")]
    public Task MockAsync(
        [Summary("zprava", "Zpráva k mockování")]
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
