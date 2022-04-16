using GrillBot.Data.Models.Suggestion;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Suggestion;

public class SuggestionSessionService
{
    private List<SuggestionMetadata> Metadata { get; } = new();
    private readonly object MetadataLock = new();

    public void InitSuggestion(string suggestionId, SuggestionType type, object data)
    {
        var metadata = new SuggestionMetadata()
        {
            Data = data,
            Type = type,
            Id = suggestionId,
            CreatedAt = DateTime.Now
        };

        lock (MetadataLock)
        {
            Metadata.Add(metadata);
        }
    }

    public SuggestionMetadata PopMetadata(SuggestionType type, string suggestionId)
    {
        lock (MetadataLock)
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
        lock (MetadataLock)
        {
            if (Metadata.Count == 0)
                return;

            var hoursBack = DateTime.Now.AddDays(-1);
            Metadata.RemoveAll(o => o.CreatedAt <= hoursBack);
        }
    }
}
