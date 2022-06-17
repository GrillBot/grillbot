using GrillBot.Data.Models.Suggestion;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Suggestion;

public class SuggestionSessionService
{
    private List<SuggestionMetadata> Metadata { get; } = new();
    private readonly object _metadataLock = new();

    public void InitSuggestion(string suggestionId, SuggestionType type, object data)
    {
        var metadata = new SuggestionMetadata()
        {
            Data = data,
            Type = type,
            Id = suggestionId,
            CreatedAt = DateTime.Now
        };

        lock (_metadataLock)
        {
            Metadata.Add(metadata);
        }
    }

    public SuggestionMetadata PopMetadata(SuggestionType type, string suggestionId)
    {
        lock (_metadataLock)
        {
            var metadata = Metadata.Find(o => o.Type == type && o.Id == suggestionId);
            if (metadata == null)
                return null;

            Metadata.RemoveAll(o => o.Type == type && o.Id == suggestionId);
            return metadata;
        }
    }

    public void PurgeExpired()
    {
        lock (_metadataLock)
        {
            if (Metadata.Count == 0)
                return;

            var hoursBack = DateTime.Now.AddDays(-1);
            Metadata.RemoveAll(o => o.CreatedAt <= hoursBack);
        }
    }
}
