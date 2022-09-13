using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Data.Models.Duck;
using System.Net.Http;
using GrillBot.App.Infrastructure.Commands;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
public class DuckModule : InteractionsModuleBase
{
    private IHttpClientFactory HttpClientFactory { get; }
    private IConfiguration Configuration { get; }
    private CultureInfo Culture { get; }

    public DuckModule(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        HttpClientFactory = httpClientFactory;
        Configuration = configuration;
        Culture = new CultureInfo("cs-CZ");
    }

    [SlashCommand("duck", "Finds the current state of the duck club.")]
    public async Task GetDuckInfoAsync()
    {
        var currentState = await GetCurrentStateAsync();
        if (currentState == null)
        {
            var infoChannel = Configuration.GetValue<string>("Services:KachnaOnline:InfoChannel");
            await SetResponseAsync($"Nepodařilo se zjistit stav kachny. Zkus to prosím později, nebo se podívej do kanálu {infoChannel}");
            return;
        }

        var embed = new EmbedBuilder()
            .WithAuthor("U Kachničky")
            .WithColor(Color.Gold)
            .WithCurrentTimestamp();

        var titleBuilder = new StringBuilder();
        switch (currentState.State)
        {
            case Data.Enums.DuckState.Private:
            case Data.Enums.DuckState.Closed:
                ProcessPrivateOrClosed(titleBuilder, currentState, embed);
                break;
            case Data.Enums.DuckState.OpenBar:
                ProcessOpenBar(titleBuilder, currentState, embed);
                break;
            case Data.Enums.DuckState.OpenChillzone:
                ProcessChillzone(titleBuilder, currentState, embed);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await SetResponseAsync(embed: embed.WithTitle(titleBuilder.ToString()).Build());
    }

    private async Task<DuckState> GetCurrentStateAsync()
    {
        var client = HttpClientFactory.CreateClient("KachnaOnline");
        var response = await client.GetAsync("states/current");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<DuckState>(json);
    }

    private void ProcessPrivateOrClosed(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        titleBuilder.AppendLine("Kachna je zavřená.");

        if (state.FollowingState != null)
        {
            FormatWithNextOpening(titleBuilder, state, embedBuilder);
            return;
        }

        if (state.FollowingState != null && state.State != Data.Enums.DuckState.Private)
        {
            FormatWithNextOpeningNoPrivate(state, embedBuilder);
            return;
        }

        titleBuilder.Append("Další otvíračka není naplánovaná.");
        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private void FormatWithNextOpening(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        var left = state.FollowingState.Start - DateTime.Now;

        titleBuilder
            .Append("Další otvíračka bude za ")
            .Append(left.Humanize(culture: Culture, precision: int.MaxValue, minUnit: TimeUnit.Minute))
            .Append('.');

        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private static void FormatWithNextOpeningNoPrivate(DuckState state, EmbedBuilder embed)
    {
        if (string.IsNullOrEmpty(state.Note))
        {
            embed.AddField("A co dál?",
                $"Další otvíračka není naplánovaná, ale tento stav má skončit {state.FollowingState.PlannedEnd:dd. MM. v HH:mm}. Co bude pak, to nikdo neví.");

            return;
        }

        AddNoteToEmbed(embed, state.Note, "A co dál?");
    }

    private void ProcessOpenBar(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        titleBuilder.Append("Kachna je otevřená!");
        embedBuilder.AddField("Otevřeno", state.Start.ToString("HH:mm"), true);

        if (state.PlannedEnd.HasValue)
        {
            var left = state.PlannedEnd.Value - DateTime.Now;

            titleBuilder.Append(" Do konce zbývá ").Append(left.Humanize(culture: Culture, precision: int.MaxValue, minUnit: TimeUnit.Minute)).Append('.');
            embedBuilder.AddField("Zavíráme", state.PlannedEnd.Value.ToString("HH:mm"), true);
        }

        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private static void ProcessChillzone(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        titleBuilder
            .Append("Kachna je otevřená v režimu chillzóna až do ")
            .Append($"{state.PlannedEnd!.Value:HH:mm}")
            .Append('!');

        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private static void AddNoteToEmbed(EmbedBuilder embed, string note, string title = "Poznámka")
    {
        if (!string.IsNullOrEmpty(note))
            embed.AddField(title, note);
    }
}
