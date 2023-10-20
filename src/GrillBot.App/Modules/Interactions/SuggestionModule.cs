using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.Core.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("suggestion", "Submission of proposal")]
public class SuggestionModule : InteractionsModuleBase
{
    public SuggestionModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("emote", "Submitting a proposal to add a new emote.")]
    [RequireValidEmoteSuggestionsAttribute("Aktuálně není období pro podávání návrhů na nové emoty.")]
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
        using var command = GetCommand<Actions.Commands.EmoteSuggestion.SetApprove>();
        await command.Command.ProcessAsync(approved);
    }

    [SlashCommand("process_emote_suggestions", "Processing approved emote suggestions.", true)]
    [RequireEmoteSuggestionChannel]
    public async Task ProcessEmoteSuggestionsAsync()
    {
        try
        {
            using var command = GetCommand<Actions.Commands.EmoteSuggestion.ProcessToVote>();
            await command.Command.ProcessAsync();
        }
        catch (Exception ex) when (ex is GrillBotException or NotFoundException or ValidationException)
        {
            await SetResponseAsync(ex.Message);
        }
    }
}
