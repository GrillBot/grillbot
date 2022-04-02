using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Searching;

public class SearchingMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Search";

    public ulong ChannelId { get; set; }
    public ulong GuildId { get; set; }
    public string MessageQuery { get; set; }

    public override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(ChannelId)] = ChannelId.ToString();
        destination[nameof(GuildId)] = GuildId.ToString();

        if (!string.IsNullOrEmpty(MessageQuery))
            destination[nameof(MessageQuery)] = MessageQuery;
    }

    public override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong guildId = 0;
        ulong channelId = 0;

        var success = values.TryGetValue(nameof(GuildId), out var _guildId) && ulong.TryParse(_guildId, out guildId);
        success &= values.TryGetValue(nameof(ChannelId), out var _channelId) && ulong.TryParse(_channelId, out channelId);
        values.TryGetValue(nameof(MessageQuery), out var messageQuery);

        if (success)
        {
            GuildId = guildId;
            ChannelId = channelId;
            MessageQuery = messageQuery;
            return true;
        }

        return false;
    }

    public override void Reset()
    {
        ChannelId = default;
        GuildId = default;
        MessageQuery = default;
    }
}
