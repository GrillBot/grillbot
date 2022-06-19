using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Points;

public class PointsBoardMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Points";

    public ulong GuildId { get; set; }

    protected override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(GuildId)] = GuildId.ToString();
    }

    protected override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong guildId = 0;

        var success = values.TryGetValue(nameof(GuildId), out string _guildId) && ulong.TryParse(_guildId, out guildId);

        if (success)
        {
            GuildId = guildId;
            return true;
        }

        return false;
    }

    protected override void Reset()
    {
        GuildId = default;
    }
}
