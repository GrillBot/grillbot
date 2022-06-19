using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Reminder;

public class RemindListMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Remind";

    public ulong OfUser { get; set; }

    protected override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(OfUser)] = OfUser.ToString();
    }

    protected override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong ofUser = 0;
        var success = values.TryGetValue(nameof(OfUser), out var ofUserData) && ulong.TryParse(ofUserData, out ofUser);

        if (!success)
            return false;

        OfUser = ofUser;
        return true;
    }

    protected override void Reset()
    {
        OfUser = default;
    }
}
