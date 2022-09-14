using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
public class MemeModule : InteractionsModuleBase
{
    private RandomizationService RandomizationService { get; }
    private IConfiguration Configuration { get; }

    public MemeModule(RandomizationService randomizationService, IConfiguration configuration, LocalizationManager localization) : base(localization)
    {
        RandomizationService = randomizationService;
        Configuration = configuration;
    }

    [SlashCommand("kasparek", "He asks your mom what kind of **** you have.")]
    public Task GetRandomLengthAsync()
    {
        var random = RandomizationService.GetOrCreateGenerator("Kasparek");
        var value = random.Next(0, 50);
        return SetResponseAsync($"{value}cm");
    }

    [SlashCommand("hi", "Hello user")]
    public Task HiAsync(
        [Summary("base", "Tell the bot in which base to greet you.")] [Choice("Binary", 2)] [Choice("Octal", 8)] [Choice("Hexadecimal", 16)]
        int? @base = null
    )
    {
        var emote = Configuration.GetValue<string>("Discord:Emotes:FeelsWowMan");
        var msg = GetLocale(nameof(HiAsync), "Template").FormatWith(Context.User.GetDisplayName(false), emote);

        return SetResponseAsync(@base == null ? msg : string.Join(" ", msg.Select(o => Convert.ToString(o, @base.Value))));
    }
}
