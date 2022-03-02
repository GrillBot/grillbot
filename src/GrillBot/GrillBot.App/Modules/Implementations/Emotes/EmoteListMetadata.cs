using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Emotes;

public class EmoteListMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "EmoteList";

    public ulong GuildId { get; set; }
    public string SortQuery { get; set; }
    public ulong? OfUserId { get; set; }

    public override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(SortQuery)] = SortQuery;
        destination[nameof(GuildId)] = GuildId.ToString();

        if (OfUserId != null)
            destination[nameof(OfUserId)] = OfUserId.Value.ToString();
    }

    public override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong guildId = 0;
        ulong ofUserId = 0;

        var success = values.TryGetValue(nameof(SortQuery), out string sortQuery);
        success &= values.TryGetValue(nameof(GuildId), out var _guildId) && ulong.TryParse(_guildId, out guildId);
        success &= !values.TryGetValue(nameof(OfUserId), out var _ofUserId) || ulong.TryParse(_ofUserId, out ofUserId);

        if (success)
        {
            GuildId = guildId;
            SortQuery = sortQuery;
            OfUserId = ofUserId == 0 ? null : ofUserId;
            return true;
        }

        return false;
    }

    public override void Reset()
    {
        SortQuery = default;
        GuildId = default;
        OfUserId = default;
    }
}
