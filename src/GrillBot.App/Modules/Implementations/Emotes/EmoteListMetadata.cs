using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Data.Enums;

namespace GrillBot.App.Modules.Implementations.Emotes;

public class EmoteListMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "EmoteList";

    public string OrderBy { get; set; } = null!;
    public SortType SortType { get; set; }
    public ulong? OfUserId { get; set; }
    public bool FilterAnimated { get; set; }

    protected override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(OrderBy)] = OrderBy;
        destination[nameof(SortType)] = SortType.ToString();
        destination[nameof(FilterAnimated)] = FilterAnimated.ToString();

        if (OfUserId != null)
            destination[nameof(OfUserId)] = OfUserId.Value.ToString();
    }

    protected override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong ofUserId = 0;
        var sortType = SortType.Ascending;
        var filterAnimated = false;

        var success = values.TryGetValue(nameof(OrderBy), out var orderBy) && !string.IsNullOrEmpty(orderBy);
        success &= values.TryGetValue(nameof(SortType), out var sortTypeData) && Enum.TryParse(sortTypeData, out sortType);
        success &= values.TryGetValue(nameof(FilterAnimated), out var filterAnimatedData) && bool.TryParse(filterAnimatedData, out filterAnimated);
        success &= !values.TryGetValue(nameof(OfUserId), out var ofUserIdData) || ulong.TryParse(ofUserIdData, out ofUserId);

        if (!success)
            return false;

        OrderBy = orderBy!;
        SortType = sortType;
        OfUserId = ofUserId == 0 ? null : ofUserId;
        FilterAnimated = filterAnimated;
        return true;
    }

    protected override void Reset()
    {
        SortType = default;
        OrderBy = default!;
        OfUserId = default;
        FilterAnimated = default;
    }
}
