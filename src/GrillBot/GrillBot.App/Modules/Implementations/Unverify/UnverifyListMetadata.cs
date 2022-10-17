using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Unverify;

public class UnverifyListMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "UnverifyList";

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
