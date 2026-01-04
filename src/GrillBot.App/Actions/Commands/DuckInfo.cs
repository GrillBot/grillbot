using System.Net.Http;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Services.KachnaOnline;
using GrillBot.Common.Services.KachnaOnline.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using DuckStateType = GrillBot.Common.Services.KachnaOnline.Enums.DuckState;

namespace GrillBot.App.Actions.Commands;

public class DuckInfo(
    IServiceClientExecutor<IKachnaOnlineClient> _client,
    ITextsManager _texts,
    IConfiguration _configuration,
    LoggingManager _loggingManager
) : CommandAction
{
    private string InfoChannel => _configuration.GetRequiredSection("Services:KachnaOnline:InfoChannel").Get<string>()!;

    public async Task<Embed> ProcessAsync()
    {
        try
        {
            var currentState = await _client.ExecuteRequestAsync((c, ctx) => c.GetCurrentStateAsync(ctx.CancellationToken));
            DuckState? nextBar = null, nextTearoom = null, nextAll = null;

            // In "non-bar" states, we want to include information on the next planned bar/tearoom opening
            // Sometimes, the next planned opening might be already included in the "FollowingState" property
            // of the current state - in that case, we don't have to make a call for that
            if (currentState.State is DuckStateType.Closed or DuckStateType.Private or DuckStateType.OpenAll)
            {
                if (currentState.FollowingState?.State is DuckStateType.OpenBar)
                    nextBar = currentState.FollowingState;
                else
                    nextBar = await GetNextStateAsync(DuckStateType.OpenBar, CancellationToken);

                if (currentState.FollowingState?.State is DuckStateType.OpenTearoom)
                    nextTearoom = currentState.FollowingState;
                else
                    nextTearoom = await GetNextStateAsync(DuckStateType.OpenTearoom, CancellationToken);

                if (currentState.FollowingState?.State is DuckStateType.OpenAll)
                    nextAll = currentState.FollowingState;
                // Only determine the next "OpenAll" state if we're not already in one
                else if (currentState.State != DuckStateType.OpenAll)
                    nextAll = await GetNextStateAsync(DuckStateType.OpenAll, CancellationToken);

            }

            ConvertDateTimesToUtc(currentState);
            ConvertDateTimesToUtc(nextBar);
            ConvertDateTimesToUtc(nextTearoom);
            ConvertDateTimesToUtc(nextAll);

            return CreateEmbed(currentState, nextBar, nextTearoom, nextAll);
        }
        catch (HttpRequestException ex)
        {
            await _loggingManager.ErrorAsync("DuckInfo", "An error occured while executing request on KachnaOnline.", ex);
            throw new GrillBotException(string.Format(GetText("CannotGetState"), InfoChannel));
        }
    }

    private async Task<DuckState?> GetNextStateAsync(DuckStateType type, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _client.ExecuteRequestAsync((c, ctx) => c.GetNextStateAsync(type, ctx.CancellationToken), cancellationToken);
        }
        catch (ClientNotFoundException)
        {
            await _loggingManager.InfoAsync("DuckInfo", $"No next state of type {type} found.");
            return null;
        }
    }

    private static void ConvertDateTimesToUtc(DuckState? state)
    {
        if (state == null)
            return;

        state.Start = state.Start.WithKind(DateTimeKind.Local).ToUniversalTime();

        if (state.PlannedEnd is not null)
            state.PlannedEnd = state.PlannedEnd.Value.WithKind(DateTimeKind.Local).ToUniversalTime();
    }

    private Embed CreateEmbed(DuckState currentState, DuckState? nextBar, DuckState? nextTearoom, DuckState? nextAll)
    {
        var embed = new EmbedBuilder()
            .WithAuthor(GetText("DuckName"))
            .WithColor(Color.Gold)
            .WithCurrentTimestamp();

        var titleBuilder = new StringBuilder();
        switch (currentState.State)
        {
            case DuckStateType.Private or DuckStateType.Closed:
                ProcessPrivateOrClosed(titleBuilder, currentState, nextBar, nextTearoom, nextAll, embed);
                break;
            case DuckStateType.OpenAll:
                ProcessOpenForAll(titleBuilder, currentState, nextBar, nextTearoom, embed);
                break;
            case DuckStateType.OpenBar or DuckStateType.OpenTearoom or DuckStateType.OpenEvent:
                ProcessOpen(titleBuilder, currentState, embed);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(currentState));
        }

        return embed.WithTitle(titleBuilder.ToString()).Build();
    }

    private void AddNextIfPresent(DuckState? state, string titleId, EmbedBuilder embedBuilder)
    {
        if (state == null)
        {
            embedBuilder.AddField(GetText(titleId), GetText("NotPlanned"));
        }
        else
        {
            var tag = TimestampTag.FromDateTime(state.Start);
            embedBuilder.AddField(GetText(titleId), tag.ToString(TimestampTagStyles.ShortTime), true);
        }
    }

    private void ProcessPrivateOrClosed(StringBuilder titleBuilder, DuckState state,
        DuckState? nextBar, DuckState? nextTearoom, DuckState? nextAll, EmbedBuilder embedBuilder)
    {
        titleBuilder.AppendLine(GetText("Closed"));

        if (nextAll != null)
        {
            var tag = TimestampTag.FromDateTime(nextAll.Start);
            titleBuilder.AppendFormat(GetText("NextAll"), tag.ToString(TimestampTagStyles.Relative));
        }

        AddNextIfPresent(nextBar, "NextBar", embedBuilder);
        AddNextIfPresent(nextTearoom, "NextTearoom", embedBuilder);

        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private void ProcessOpenForAll(StringBuilder titleBuilder, DuckState state,
        DuckState? nextBar, DuckState? nextTearoom, EmbedBuilder embedBuilder)
    {
        var tag = TimestampTag.FromDateTime(state.PlannedEnd!.Value);

        titleBuilder.AppendFormat(GetText("OpenAll"), tag.ToString(TimestampTagStyles.ShortTime)).AppendLine();

        AddNextIfPresent(nextBar, "NextBar", embedBuilder);
        AddNextIfPresent(nextTearoom, "NextTearoom", embedBuilder);

        embedBuilder.AddField(GetText("ForAllDescriptionTitle"), GetText("ForAllDescription"));

        AddNoteToEmbed(embedBuilder, state.Note);
    }

    private void ProcessOpen(StringBuilder titleBuilder, DuckState state, EmbedBuilder embedBuilder)
    {
        titleBuilder.AppendLine(GetText(state.State switch
        {
            DuckStateType.OpenBar => "OpenBar",
            DuckStateType.OpenTearoom => "OpenTearoom",
            DuckStateType.OpenEvent => "OpenEvent",
            _ => throw new ArgumentOutOfRangeException(nameof(state))
        }));

        if (state.PlannedEnd.HasValue)
        {
            var start = TimestampTag.FromDateTime(state.Start);
            var end = TimestampTag.FromDateTime(state.PlannedEnd.Value);

            titleBuilder.AppendFormat(GetText("TimeToClose"), end.ToString(TimestampTagStyles.Relative));
            embedBuilder.AddField(GetText("Opening"), start.ToString(TimestampTagStyles.ShortTime), true);
            embedBuilder.AddField(GetText("Closing"), end.ToString(TimestampTagStyles.ShortTime), true);
        }

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
        => _texts[$"DuckModule/GetDuckInfo/{id}", Locale];
}
