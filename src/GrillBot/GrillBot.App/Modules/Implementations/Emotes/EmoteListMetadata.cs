using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Emotes;

public class EmoteListMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "EmoteList";

    public ulong GuildId { get; set; }
    public string OrderBy { get; set; }
    public bool Descending { get; set; }
    public ulong? OfUserId { get; set; }
    public bool FilterAnimated { get; set; }

    public override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(OrderBy)] = OrderBy;
        destination[nameof(Descending)] = Descending.ToString();
        destination[nameof(GuildId)] = GuildId.ToString();
        destination[nameof(FilterAnimated)] = FilterAnimated.ToString();

        if (OfUserId != null)
            destination[nameof(OfUserId)] = OfUserId.Value.ToString();
    }

    public override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong guildId = 0;
        ulong ofUserId = 0;
        bool descending = false;
        bool filterAnimated = false;

        var success = values.TryGetValue(nameof(OrderBy), out string orderBy);
        success &= values.TryGetValue(nameof(Descending), out string _descending) && bool.TryParse(_descending, out descending);
        success &= values.TryGetValue(nameof(GuildId), out var _guildId) && ulong.TryParse(_guildId, out guildId);
        success &= values.TryGetValue(nameof(FilterAnimated), out var _filterAnimated) && bool.TryParse(_filterAnimated, out filterAnimated);
        success &= !values.TryGetValue(nameof(OfUserId), out var _ofUserId) || ulong.TryParse(_ofUserId, out ofUserId);

        if (success)
        {
            GuildId = guildId;
            OrderBy = orderBy;
            Descending = descending;
            OfUserId = ofUserId == 0 ? null : ofUserId;
            FilterAnimated = filterAnimated;
            return true;
        }

        return false;
    }

    public override void Reset()
    {
        Descending = default;
        OrderBy = default;
        GuildId = default;
        OfUserId = default;
        FilterAnimated = default;
    }
}
