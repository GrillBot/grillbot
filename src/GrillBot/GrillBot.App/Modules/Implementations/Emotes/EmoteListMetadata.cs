using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Emotes;

public class EmoteListMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "EmoteList";

    public bool IsPrivate { get; set; }
    public bool Desc { get; set; }
    public string SortBy { get; set; }
    public ulong? OfUserId { get; set; }

    public override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(IsPrivate)] = IsPrivate.ToString();
        destination[nameof(Desc)] = Desc.ToString();
        destination[nameof(SortBy)] = SortBy;

        if (OfUserId != null)
            destination[nameof(OfUserId)] = OfUserId.Value.ToString();
    }

    public override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        bool isPrivate = false;
        bool desc = false;
        ulong ofUserId = 0;

        var success = values.TryGetValue(nameof(IsPrivate), out var _isPrivate) && bool.TryParse(_isPrivate, out isPrivate);
        success &= values.TryGetValue(nameof(Desc), out var _desc) && bool.TryParse(_desc, out desc);
        success &= values.TryGetValue(nameof(SortBy), out string sortBy);
        success &= !values.TryGetValue(nameof(OfUserId), out var _ofUserId) || ulong.TryParse(_ofUserId, out ofUserId);

        if (success)
        {
            IsPrivate = isPrivate;
            Desc = desc;
            SortBy = sortBy;
            OfUserId = ofUserId == 0 ? null : ofUserId;
            return true;
        }

        return false;
    }

    public override void Reset()
    {
        IsPrivate = default;
        Desc = default;
        SortBy = default;
        OfUserId = default;
    }
}
