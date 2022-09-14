using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Data.Models.Duck;
using System.Net.Http;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.Common.Managers;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
public class DuckModule : InteractionsModuleBase
{
    private IHttpClientFactory HttpClientFactory { get; }
    private IConfiguration Configuration { get; }

    public DuckModule(IHttpClientFactory httpClientFactory, IConfiguration configuration, LocalizationManager localization) : base(localization)
    {
        HttpClientFactory = httpClientFactory;
        Configuration = configuration;
    }

    [SlashCommand("duck", "Finds the current state of the duck club.")]
    public async Task GetDuckInfoAsync()
    {
        var currentState = await GetCurrentStateAsync();
        if (currentState == null)
        {
            var infoChannel = Configuration.GetValue<string>("Services:KachnaOnline:InfoChannel");
            await SetResponseAsync(GetLocale(nameof(GetDuckInfoAsync), "CannotGetState").FormatWith(infoChannel));
            return;
        }

        var embed = new EmbedBuilder()
            .WithAuthor(GetLocale(nameof(GetDuckInfoAsync), "DuckName"))
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
        titleBuilder.AppendLine(GetLocale(nameof(GetDuckInfoAsync), "Closed"));

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

        titleBuilder.Append(GetLocale(nameof(GetDuckInfoAsync), "NextOpenNotPlanned"));
        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private void FormatWithNextOpening(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        var left = state.FollowingState.Start - DateTime.Now;

        titleBuilder
            .Append(GetLocale(nameof(GetDuckInfoAsync), "NextOpenAt").FormatWith(left.Humanize(culture: Culture, precision: int.MaxValue, minUnit: TimeUnit.Minute)));

        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private void FormatWithNextOpeningNoPrivate(DuckState state, EmbedBuilder embed)
    {
        if (string.IsNullOrEmpty(state.Note))
        {
            embed.AddField(GetLocale(nameof(GetDuckInfoAsync), "WhatsNext"),
                GetLocale(nameof(GetDuckInfoAsync), "WhatsNextUnknown").FormatWith(state.FollowingState.PlannedEnd!.Value.ToString("dd. MM. HH:mm")));

            return;
        }

        AddNoteToEmbed(embed, state.Note, GetLocale(nameof(GetDuckInfoAsync), "WhatsNext"));
    }

    private void ProcessOpenBar(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        titleBuilder.Append(GetLocale(nameof(GetDuckInfoAsync), "Opened"));
        embedBuilder.AddField(GetLocale(nameof(GetDuckInfoAsync), "Open"), state.Start.ToString("HH:mm"), true);

        if (state.PlannedEnd.HasValue)
        {
            var left = state.PlannedEnd.Value - DateTime.Now;

            titleBuilder.Append(GetLocale(nameof(GetDuckInfoAsync), "TimeToClose").FormatWith(left.Humanize(culture: Culture, precision: int.MaxValue, minUnit: TimeUnit.Minute)));
            embedBuilder.AddField(GetLocale(nameof(GetDuckInfoAsync), "Closing"), state.PlannedEnd.Value.ToString("HH:mm"), true);
        }

        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private void ProcessChillzone(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        titleBuilder
            .Append(GetLocale(nameof(GetDuckInfoAsync), "ChillzoneTo").FormatWith(state.PlannedEnd!.Value.ToString("HH:mm")));

        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private void AddNoteToEmbed(EmbedBuilder embed, string note, string title = null)
    {
        if (string.IsNullOrEmpty(title))
            title = GetLocale(nameof(GetDuckInfoAsync), "Note");

        if (!string.IsNullOrEmpty(note))
            embed.AddField(title, note);
    }
}
