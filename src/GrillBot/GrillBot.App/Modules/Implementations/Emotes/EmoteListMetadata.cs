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

    protected override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(OrderBy)] = OrderBy;
        destination[nameof(Descending)] = Descending.ToString();
        destination[nameof(GuildId)] = GuildId.ToString();
        destination[nameof(FilterAnimated)] = FilterAnimated.ToString();

        if (OfUserId != null)
            destination[nameof(OfUserId)] = OfUserId.Value.ToString();
    }

    protected override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong guildId = 0;
        ulong ofUserId = 0;
        var descending = false;
        var filterAnimated = false;

        var success = values.TryGetValue(nameof(OrderBy), out var orderBy);
        success &= values.TryGetValue(nameof(Descending), out var descendingData) && bool.TryParse(descendingData, out descending);
        success &= values.TryGetValue(nameof(GuildId), out var guildIdData) && ulong.TryParse(guildIdData, out guildId);
        success &= values.TryGetValue(nameof(FilterAnimated), out var filterAnimatedData) && bool.TryParse(filterAnimatedData, out filterAnimated);
        success &= !values.TryGetValue(nameof(OfUserId), out var ofUserIdData) || ulong.TryParse(ofUserIdData, out ofUserId);

        if (!success)
            return false;

        GuildId = guildId;
        OrderBy = orderBy;
        Descending = descending;
        OfUserId = ofUserId == 0 ? null : ofUserId;
        FilterAnimated = filterAnimated;
        return true;
    }

    protected override void Reset()
    {
        Descending = default;
        OrderBy = default;
        GuildId = default;
        OfUserId = default;
        FilterAnimated = default;
    }
}
