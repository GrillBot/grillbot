using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Suggestion;

public class SuggestionService : ServiceBase
{
    public EmoteSuggestionService Emotes { get; }
    public FeatureSuggestionService Features { get; }
    public SuggestionSessionService Sessions { get; }

    public SuggestionService(EmoteSuggestionService emoteSuggestionService, FeatureSuggestionService featureSuggestionService,
        IDiscordClient discordClient, SuggestionSessionService suggestionSessionService) : base(null, null, discordClient)
    {
        Emotes = emoteSuggestionService;
        Features = featureSuggestionService;
        Sessions = suggestionSessionService;
    }

    public async Task ProcessPendingSuggestion(Database.Entity.Suggestion suggestion)
    {
        switch (suggestion.Type)
        {
            case SuggestionType.Emote:
                var guild = await DcClient.GetGuildAsync(suggestion.GuildId.ToUlong());
                await Emotes.TrySendSuggestionAsync(guild, suggestion);
                break;
            case SuggestionType.FeatureRequest:
                await Features.TrySendSuggestionAsync(suggestion);
                break;
        }
    }
}
