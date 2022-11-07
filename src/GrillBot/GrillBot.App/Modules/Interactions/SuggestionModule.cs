using Discord.Interactions;
using Discord.Net;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.App.Services.Suggestion;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("suggestion", "Submission of proposal")]
public class SuggestionModule : InteractionsModuleBase
{
    private EmoteSuggestionService EmoteSuggestions { get; }

    public SuggestionModule(EmoteSuggestionService emoteSuggestionService, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        EmoteSuggestions = emoteSuggestionService;
    }

    [SlashCommand("emote", "Submitting a proposal to add a new emote.")]
    [RequireValidEmoteSuggestions("Aktuálně není období pro podávání návrhů na nové emoty.")]
    [DeferConfiguration(SuppressAuto = true)]
    public async Task SuggestEmoteAsync(
        [Summary("emote", "Option to design an emote based on an existing emote (from another server).")]
        IEmote emote = null,
        [Summary("attachment", "Ability to design an emote based on an image.")]
        IAttachment attachment = null
    )
    {
        switch (emote)
        {
            case null when attachment == null:
                await SetResponseAsync(GetText(nameof(SuggestEmoteAsync), "NoEmoteAndAttachment"));
                return;
            case Emote emoteData when Context.Guild.Emotes.Any(o => o.Id == emoteData.Id):
                await SetResponseAsync(GetText(nameof(SuggestEmoteAsync), "EmoteExistsInGuild"));
                return;
        }

        var sessionId = Guid.NewGuid().ToString();
        var modal = new ModalBuilder(GetText(nameof(SuggestEmoteAsync), "ModalTitle"), $"suggestions_emote:{sessionId}")
            .AddTextInput(GetText(nameof(SuggestEmoteAsync), "ModalEmoteName"), "suggestions_emote_name", minLength: 2, maxLength: 50, required: true,
                value: emote?.Name ?? Path.GetFileNameWithoutExtension(attachment!.Filename))
            .AddTextInput(GetText(nameof(SuggestEmoteAsync), "ModalEmoteDescription"), "suggestions_emote_description", TextInputStyle.Paragraph,
                GetText(nameof(SuggestEmoteAsync), "ModalEmoteDescriptionPlaceholder"), maxLength: EmbedFieldBuilder.MaxFieldValueLength, required: false)
            .Build();

        var suggestionData = emote as object ?? attachment;
        EmoteSuggestions.InitSession(sessionId, suggestionData);

        await RespondWithModalAsync(modal);
    }

    [ModalInteraction("suggestions_emote:*", ignoreGroupNames: true)]
    public async Task EmoteSuggestionFormSubmitedAsync(string sessionId, EmoteSuggestionModal modal)
    {
        try
        {
            var user = Context.User as IGuildUser ?? Context.Guild.GetUser(Context.User.Id);
            await EmoteSuggestions.ProcessSessionAsync(sessionId, Context.Guild, user, modal);
            await Context.User.SendMessageAsync(GetText(nameof(EmoteSuggestionFormSubmitedAsync), "Success"));
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            // User have blocked DMs from bots. User problem, not ours.
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case HttpException { DiscordCode: DiscordErrorCode.CannotSendMessageToUser }:
                    return;
                case ValidationException:
                case NotFoundException:
                    try
                    {
                        await Context.User.SendMessageAsync(ex.Message);
                    }
                    catch (HttpException ex1) when (ex1.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                    {
                        // User have blocked DMs from bots. User problem, not ours.
                    }

                    break;
            }
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
