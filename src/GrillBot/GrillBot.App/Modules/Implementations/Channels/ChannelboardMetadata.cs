using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Channels;

public class ChannelboardMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Channelboard";

    public ulong GuildId { get; set; }

    public override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(GuildId)] = GuildId.ToString();
    }

    public override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        if (values.TryGetValue(nameof(GuildId), out string _guildId) && ulong.TryParse(_guildId, out var guildId))
        {
            GuildId = guildId;
            return true;
        }

        return false;
    }

    public override void Reset()
    {
        GuildId = default;
    }
}
