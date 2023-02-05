using System.Net.Http;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Services.KachnaOnline;
using GrillBot.Common.Services.KachnaOnline.Models;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Actions.Commands;

public class DuckInfo : CommandAction
{
    private IKachnaOnlineClient Client { get; }
    private ITextsManager Texts { get; }
    private IConfiguration Configuration { get; }
    private LoggingManager LoggingManager { get; }

    private string InfoChannel => Configuration.GetRequiredSection("Services:KachnaOnline:InfoChannel").Get<string>()!;
    private CultureInfo Culture => Texts.GetCulture(Locale);

    public DuckInfo(IKachnaOnlineClient client, ITextsManager texts, IConfiguration configuration, LoggingManager loggingManager)
    {
        Client = client;
        Texts = texts;
        Configuration = configuration;
        LoggingManager = loggingManager;
    }

    public async Task<Embed> ProcessAsync()
    {
        try
        {
            var currentState = await Client.GetCurrentStateAsync();
            return CreateEmbed(currentState);
        }
        catch (HttpRequestException ex)
        {
            await LoggingManager.ErrorAsync("DuckInfo", "An error occured while executing request on KachnaOnline.", ex);
            throw new GrillBotException(GetText("CannotGetState").FormatWith(InfoChannel));
        }
    }

    private Embed CreateEmbed(DuckState currentState)
    {
        var embed = new EmbedBuilder()
            .WithAuthor(GetText("DuckName"))
            .WithColor(Color.Gold)
            .WithCurrentTimestamp();

        var titleBuilder = new StringBuilder();
        switch (currentState.State)
        {
            case Common.Services.KachnaOnline.Enums.DuckState.Private or Common.Services.KachnaOnline.Enums.DuckState.Closed:
                ProcessPrivateOrClosed(titleBuilder, currentState, embed);
                break;
            case Common.Services.KachnaOnline.Enums.DuckState.OpenBar:
                ProcessOpenBar(titleBuilder, currentState, embed);
                break;
            case Common.Services.KachnaOnline.Enums.DuckState.OpenChillzone:
                ProcessChillzone(titleBuilder, currentState, embed);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentState));
        }

        return embed.WithTitle(titleBuilder.ToString()).Build();
    }

    private void ProcessPrivateOrClosed(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        titleBuilder.AppendLine(GetText("Closed"));

        if (state.FollowingState is { State: Common.Services.KachnaOnline.Enums.DuckState.OpenBar })
        {
            FormatWithNextOpening(titleBuilder, state, embedBuilder);
            return;
        }

        titleBuilder.Append(GetText("NextOpenNotPlanned"));
        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private void FormatWithNextOpening(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        var left = state.FollowingState!.Start - DateTime.Now;

        titleBuilder
            .Append(GetText("NextOpenAt").FormatWith(left.Humanize(culture: Culture, precision: int.MaxValue, minUnit: TimeUnit.Minute)));

        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private void ProcessOpenBar(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        titleBuilder.Append(GetText("Opened"));
        embedBuilder.AddField(GetText("Open"), state.Start.ToString("HH:mm"), true);

        if (state.PlannedEnd.HasValue)
        {
            var left = state.PlannedEnd.Value - DateTime.Now;

            titleBuilder.Append(GetText("TimeToClose").FormatWith(left.Humanize(culture: Culture, precision: int.MaxValue, minUnit: TimeUnit.Minute)));
            embedBuilder.AddField(GetText("Closing"), state.PlannedEnd.Value.ToString("HH:mm"), true);
        }

        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private void ProcessChillzone(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        titleBuilder
            .Append(GetText("ChillzoneTo").FormatWith(state.PlannedEnd!.Value.ToString("HH:mm")));

        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private void AddNoteToEmbed(EmbedBuilder embed, string? note, string? title = null)
    {
        if (string.IsNullOrEmpty(title))
            title = GetText("Note");

        if (!string.IsNullOrEmpty(note))
            embed.AddField(title, note);
    }

    private string GetText(string id)
        => Texts[$"DuckModule/GetDuckInfo/{id}", Locale];
}
