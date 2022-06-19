using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
public class MemeModule : InteractionsModuleBase
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

        return SetResponseAsync(@base == null ? msg : string.Join(" ", msg.Select(o => Convert.ToString(o, @base.Value))));
    }
}
