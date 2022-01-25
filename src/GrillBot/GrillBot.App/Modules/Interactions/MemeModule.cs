using Discord.Interactions;
using GrillBot.App.Services;

namespace GrillBot.App.Modules.Interactions;

public class MemeModule : Infrastructure.InteractionsModuleBase
{
    private RandomizationService RandomizationService { get; }

    public MemeModule(RandomizationService randomizationService)
    {
        RandomizationService = randomizationService;
    }

    [SlashCommand("kasparek", "Zeptá se tvojí mámy, jakého máš kašpárka.")]
    public Task GetRandomLengthAsync()
    {
        var random = RandomizationService.GetOrCreateGenerator("Kasparek");
        var value = random.Next(0, 50);
        return SetResponseAsync($"{value}cm");
    }
}
