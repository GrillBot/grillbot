using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Channels;

public class ChannelboardMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Channelboard";

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
