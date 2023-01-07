using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Points;

public class PointsBoardMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Points";

    protected override void Save(IDictionary<string, string> destination)
    {
    }

    protected override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        return true;
    }

    protected override void Reset()
    {
    }
}
