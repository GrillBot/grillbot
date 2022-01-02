using GrillBot.App.Infrastructure.Embeds;
using System.Collections.Generic;

namespace GrillBot.App.Modules.Implementations.Searching;

public class SearchingMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Search";

    public ulong ChannelId { get; set; }
    public ulong GuildId { get; set; }

    public override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(ChannelId)] = ChannelId.ToString();
        destination[nameof(GuildId)] = GuildId.ToString();
    }

    public override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong guildId = 0;
        ulong channelId = 0;

        var success = values.TryGetValue(nameof(GuildId), out var _guildId) && ulong.TryParse(_guildId, out guildId);
        success &= values.TryGetValue(nameof(ChannelId), out var _channelId) && ulong.TryParse(_channelId, out channelId);

        if (success)
        {
            GuildId = guildId;
            ChannelId = channelId;
            return true;
        }

        return false;
    }

    public override void Reset()
    {
        ChannelId = default;
        GuildId = default;
    }
}
