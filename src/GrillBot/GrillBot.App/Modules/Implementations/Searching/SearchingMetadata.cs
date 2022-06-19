using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Searching;

public class SearchingMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Search";

    public ulong ChannelId { get; set; }
    public ulong GuildId { get; set; }
    public string MessageQuery { get; set; }

    protected override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(ChannelId)] = ChannelId.ToString();
        destination[nameof(GuildId)] = GuildId.ToString();

        if (!string.IsNullOrEmpty(MessageQuery))
            destination[nameof(MessageQuery)] = MessageQuery;
    }

    protected override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong guildId = 0;
        ulong channelId = 0;

        var success = values.TryGetValue(nameof(GuildId), out var guildIdData) && ulong.TryParse(guildIdData, out guildId);
        success &= values.TryGetValue(nameof(ChannelId), out var channelIdData) && ulong.TryParse(channelIdData, out channelId);
        values.TryGetValue(nameof(MessageQuery), out var messageQuery);

        if (!success)
            return false;

        GuildId = guildId;
        ChannelId = channelId;
        MessageQuery = messageQuery;
        return true;
    }

    protected override void Reset()
    {
        ChannelId = default;
        GuildId = default;
        MessageQuery = default;
    }
}
