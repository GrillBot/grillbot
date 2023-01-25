using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Managers.EmoteSuggestion;
using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("suggestion", "Submission of proposal")]
[ExcludeFromCodeCoverage]
public class SuggestionModule : InteractionsModuleBase
{
    private EmoteSuggestionManager EmoteSuggestions { get; }

    public SuggestionModule(EmoteSuggestionManager emoteSuggestionManager, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        EmoteSuggestions = emoteSuggestionManager;
    }

    [SlashCommand("emote", "Submitting a proposal to add a new emote.")]
    [RequireValidEmoteSuggestions("Aktuálně není období pro podávání návrhů na nové emoty.")]
    [DeferConfiguration(SuppressAuto = true)]
    public async Task SuggestEmoteAsync(
        [Summary("emote", "Option to design an emote based on an existing emote (from another server).")]
        IEmote? emote = null,
        [Summary("attachment", "Ability to design an emote based on an image.")]
        IAttachment? attachment = null
    )
    {
        using var command = GetCommand<Actions.Commands.EmoteSuggestion.InitSuggestion>();

        try
        {
            var modal = await command.Command.ProcessAsync(emote, attachment);
            await RespondWithModalAsync(modal);
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [ModalInteraction("suggestions_emote:*", ignoreGroupNames: true)]
    public async Task EmoteSuggestionFormSubmitedAsync(string sessionId, EmoteSuggestionModal modal)
    {
        try
        {
            using var command = GetCommand<Actions.Commands.EmoteSuggestion.FormSubmitted>();
            await command.Command.ProcessAsync(sessionId, modal);
        }
        finally
        {
            await DeferAsync();
        }
    }

    [ComponentInteraction("emote_suggestion_approve:*", ignoreGroupNames: true)]
    public async Task EmoteSuggestionApproved(bool approved)
    {
        await EmoteSuggestions.SetApprovalStateAsync((IComponentInteraction)Context.Interaction, approved, Context.Channel);
    }

    [SlashCommand("process_emote_suggestions", "Processing approved emote suggestions.", true)]
    [RequireEmoteSuggestionChannel]
    public async Task ProcessEmoteSuggestionsAsync()
    {
        try
        {
            await EmoteSuggestions.ProcessSuggestionsToVoteAsync(Context.Guild);
            await SetResponseAsync(GetText(nameof(ProcessEmoteSuggestionsAsync), "Success"));
        }
        catch (GrillBotException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }
}
