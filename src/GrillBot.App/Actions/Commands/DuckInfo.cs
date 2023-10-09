using System.Net.Http;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Services.KachnaOnline;
using GrillBot.Common.Services.KachnaOnline.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;

namespace GrillBot.App.Actions.Commands;

public class DuckInfo : CommandAction
{
    private IKachnaOnlineClient Client { get; }
    private ITextsManager Texts { get; }
    private IConfiguration Configuration { get; }
    private LoggingManager LoggingManager { get; }

    private string InfoChannel => Configuration.GetRequiredSection("Services:KachnaOnline:InfoChannel").Get<string>()!;

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
            ConvertDateTimesToUtc(currentState);

            return CreateEmbed(currentState);
        }
        catch (HttpRequestException ex)
        {
            await LoggingManager.ErrorAsync("DuckInfo", "An error occured while executing request on KachnaOnline.", ex);
            throw new GrillBotException(GetText("CannotGetState").FormatWith(InfoChannel));
        }
    }

    private static void ConvertDateTimesToUtc(DuckState state)
    {
        state.Start = state.Start.WithKind(DateTimeKind.Local).ToUniversalTime();

        if (state.PlannedEnd is not null)
            state.PlannedEnd = state.PlannedEnd.Value.WithKind(DateTimeKind.Local).ToUniversalTime();

        if (state.FollowingState is not null)
            ConvertDateTimesToUtc(state.FollowingState);
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
        var tag = TimestampTag.FromDateTime(state.FollowingState!.Start);

        titleBuilder.Append(GetText("NextOpenAt").FormatWith(tag.ToString(TimestampTagStyles.Relative)));
        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private void ProcessOpenBar(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        titleBuilder.Append(GetText("Opened"));

        var openingAt = TimestampTag.FromDateTimeOffset(state.Start);
        embedBuilder.AddField(GetText("Open"), openingAt.ToString(TimestampTagStyles.ShortTime), true);

        if (state.PlannedEnd.HasValue)
        {
            var tag = TimestampTag.FromDateTime(state.PlannedEnd.Value);

            titleBuilder.Append(GetText("TimeToClose").FormatWith(tag.ToString(TimestampTagStyles.Relative)));
            embedBuilder.AddField(GetText("Closing"), tag.ToString(TimestampTagStyles.ShortTime), true);
        }

        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private void ProcessChillzone(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        var closingAt = TimestampTag.FromDateTime(state.PlannedEnd!.Value);

        titleBuilder.Append(GetText("ChillzoneTo").FormatWith(closingAt.ToString(TimestampTagStyles.ShortTime)));
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
