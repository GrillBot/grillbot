using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Points;

public class PointsBoardMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Points";

    public bool OverAllTime { get; set; }

    protected override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(OverAllTime)] = OverAllTime.ToString();
    }

    protected override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        var overAllTime = false;

        var success = values.TryGetValue(nameof(OverAllTime), out var _overAllTime) && bool.TryParse(_overAllTime, out overAllTime);
        if (!success)
            return false;

        OverAllTime = overAllTime;
        return true;
    }

    protected override void Reset()
    {
        OverAllTime = default;
    }
}
