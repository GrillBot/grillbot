using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Channels;

public class ChannelboardMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Channelboard";

    public ulong GuildId { get; set; }

    protected override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(GuildId)] = GuildId.ToString();
    }

    protected override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        if (!values.TryGetValue(nameof(GuildId), out var guildIdData) || !ulong.TryParse(guildIdData, out var guildId))
            return false;

        GuildId = guildId;
        return true;
    }

    protected override void Reset()
    {
        GuildId = default;
    }
}
