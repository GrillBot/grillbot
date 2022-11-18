using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Searching;

public class SearchingMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Search";

    public ulong ChannelId { get; set; }
    public string MessageQuery { get; set; }

    protected override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(ChannelId)] = ChannelId.ToString();

        if (!string.IsNullOrEmpty(MessageQuery))
            destination[nameof(MessageQuery)] = MessageQuery;
    }

    protected override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong channelId = 0;

        var success = values.TryGetValue(nameof(ChannelId), out var channelIdData) && ulong.TryParse(channelIdData, out channelId);
        values.TryGetValue(nameof(MessageQuery), out var messageQuery);

        if (!success)
            return false;

        ChannelId = channelId;
        MessageQuery = messageQuery;
        return true;
    }

    protected override void Reset()
    {
        ChannelId = default;
        MessageQuery = default;
    }
}
