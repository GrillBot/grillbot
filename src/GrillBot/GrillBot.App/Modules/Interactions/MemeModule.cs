using Discord.Interactions;
using GrillBot.App.Services;
using GrillBot.Data.Extensions.Discord;

namespace GrillBot.App.Modules.Interactions;

public class MemeModule : Infrastructure.InteractionsModuleBase
{
    private RandomizationService RandomizationService { get; }
    private IConfiguration Configuration { get; }

    public MemeModule(RandomizationService randomizationService, IConfiguration configuration)
    {
        RandomizationService = randomizationService;
        Configuration = configuration;
    }

    [SlashCommand("kasparek", "Zeptá se tvojí mámy, jakého máš kašpárka.")]
    public Task GetRandomLengthAsync()
    {
        var random = RandomizationService.GetOrCreateGenerator("Kasparek");
        var value = random.Next(0, 50);
        return SetResponseAsync($"{value}cm");
    }

    [SlashCommand("hi", "Pozdrav uživatele")]
    public Task HiAsync(
        [Summary("zaklad", "Řekni botovi, v jaké soustavě tě má podravit.")]
        [Choice("Binární", 2)]
        [Choice("Osmičková", 8)]
        [Choice("Šestnáctková", 16)]
        int? @base = null
    )
    {
        var emote = Configuration.GetValue<string>("Discord:Emotes:FeelsWowMan");
        var msg = $"Ahoj {Context.User.GetDisplayName(false)} {emote}";

        if (@base == null)
            return SetResponseAsync(msg);
        else
            return SetResponseAsync(string.Join(" ", msg.Select(o => Convert.ToString(o, @base.Value))));
    }
}
