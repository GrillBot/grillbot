using Discord.Interactions;
using Discord.Net;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.App.Services.Suggestion;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("suggestion", "Podání návrhu")]
public class SuggestionModule : InteractionsModuleBase
{
    private SuggestionService SuggestionService { get; }

    public SuggestionModule(SuggestionService suggestionService)
    {
        SuggestionService = suggestionService;
        CanDefer = false;
    }

    [SlashCommand("emote", "Podání návrhu na přidání nového emote.")]
    public async Task SuggestEmoteAsync(
        [Summary("emote", "Možnost navrhnout emote na základě existujícího emote (z jiného serveru).")]
        IEmote emote = null,
        [Summary("attachment", "Možnost navrhnout emote na základě obrázku.")]
        IAttachment attachment = null
    )
    {
        if (emote == null && attachment == null)
        {
            await SetResponseAsync("Nelze podat návrh na nový emote, když není dodán emote. Emote lze dodat ve formě jiného emote, nebo obrázku.");
            return;
        }

        var sessionId = Guid.NewGuid().ToString();
        var modal = new ModalBuilder("Podání návrhu na nový emote", $"suggestions_emote:{sessionId}")
            .AddTextInput("Název emote", "suggestions_emote_name", minLength: 2, maxLength: 50, required: true, value: emote?.Name ?? Path.GetFileNameWithoutExtension(attachment!.Filename))
            .AddTextInput("Popis emote", "suggestions_emote_description", TextInputStyle.Paragraph, "Něco k doplnění? Co emote vyjadřuje? Proč bychom ho tu měli mít?", maxLength: 1500, required: false)
            .Build();

        var suggestionData = emote as object ?? attachment;
        SuggestionService.Emotes.InitSession(sessionId, suggestionData);

        await RespondWithModalAsync(modal);
    }

    [ModalInteraction("suggestions_emote:*", ignoreGroupNames: true)]
    public async Task EmoteSuggestionFormSubmitedAsync(string sessionId, EmoteSuggestionModal modal)
    {
        try
        {
            await SuggestionService.Emotes.ProcessSessionAsync(sessionId, Context.Guild, Context.User, modal);
            await Context.User.SendMessageAsync("Tvůj návrh na přidání emote byl úspěšně zpracován.");
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

    [SlashCommand("feature", "Podání návrhu na novou feature do GrillBot.")]
    public Task SuggestFeatureAsync()
    {
        var suggestionId = Guid.NewGuid().ToString();
        SuggestionService.Features.InitSession(suggestionId);

        return RespondWithModalAsync<FeatureSuggestionModal>($"suggestions_feature:{suggestionId}");
    }

    [ModalInteraction("suggestions_feature:*", ignoreGroupNames: true)]
    public async Task FeatureSuggestionSubmittedAsync(string sessionId, FeatureSuggestionModal modal)
    {
        try
        {
            await SuggestionService.Features.ProcessSessionAsync(sessionId, Context.Guild, Context.User, modal);
            await Context.User.SendMessageAsync("Tvůj návrh na přidání feature byl úspěšně zpracován.");
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
}
