using Discord;
using Discord.Commands;
using Discord.Interactions;
using GrillBot.Data.Services;
using System.Threading.Tasks;

namespace GrillBot.Data.Modules.Interactions;

public class MockingModule : Infrastructure.InteractionsModuleBase
{
    private MockingService MockingService { get; }

    public MockingModule(MockingService mockingService)
    {
        MockingService = mockingService;
    }

    [SlashCommand("mock", "Mockuje zadanou zprávu")]
    public Task MockAsync(
        [Remainder]
        [Name("zpráva")]
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
