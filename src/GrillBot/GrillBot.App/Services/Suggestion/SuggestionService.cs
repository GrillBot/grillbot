using GrillBot.Common.Extensions;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Suggestion;

public class SuggestionService
{
    public EmoteSuggestionService Emotes { get; }
    public FeatureSuggestionService Features { get; }
    public SuggestionSessionService Sessions { get; }
    private IDiscordClient DiscordClient { get; }

    public SuggestionService(EmoteSuggestionService emoteSuggestionService, FeatureSuggestionService featureSuggestionService,
        IDiscordClient discordClient, SuggestionSessionService suggestionSessionService)
    {
        Emotes = emoteSuggestionService;
        Features = featureSuggestionService;
        Sessions = suggestionSessionService;
        DiscordClient = discordClient;
    }

    public async Task ProcessPendingSuggestion(Database.Entity.Suggestion suggestion)
    {
        // TODO Rewrite
        return;
        switch (suggestion.Type)
        {
            case SuggestionType.VotableEmote:
                var guild = await DiscordClient.GetGuildAsync(suggestion.GuildId.ToUlong());
                //await Emotes.TrySendSuggestionAsync(guild, suggestion);
                break;
            case SuggestionType.FeatureRequest:
                await Features.TrySendSuggestionAsync(suggestion);
                break;
            default:
                throw new ArgumentOutOfRangeException("", nameof(suggestion.Type));
        }
    }
}
