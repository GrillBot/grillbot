using GrillBot.App.Infrastructure;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Suggestion;

public class SuggestionService : ServiceBase
{
    public EmoteSuggestionService Emotes { get; }
    public FeatureSuggestionService Features { get; }
    public SuggestionSessionService Sessions { get; }

    public SuggestionService(EmoteSuggestionService emoteSuggestionService, FeatureSuggestionService featureSuggestionService,
        IDiscordClient discordClient, SuggestionSessionService suggestionSessionService) : base(null, null, null, discordClient)
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
                var guildId = Convert.ToUInt64(suggestion.GuildId);
                var guild = await DcClient.GetGuildAsync(guildId);
                await Emotes.TrySendSuggestionAsync(guild, suggestion);
                break;
            case SuggestionType.FeatureRequest:
                await Features.TrySendSuggestionAsync(suggestion);
                break;
        }
    }
}
