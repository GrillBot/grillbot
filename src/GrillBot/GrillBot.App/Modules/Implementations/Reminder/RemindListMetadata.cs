using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Reminder;

public class RemindListMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Remind";

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
