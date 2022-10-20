using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.User;

public class UserAccessListMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "UserAccessList";

    public ulong ForUserId { get; set; }

    protected override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(ForUserId)] = ForUserId.ToString();
    }

    protected override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong forUserId = 0;

        var success = values.TryGetValue(nameof(ForUserId), out var forUserIdData) && ulong.TryParse(forUserIdData, out forUserId);

        if (!success)
            return false;

        ForUserId = forUserId;
        return true;
    }

    protected override void Reset()
    {
        ForUserId = default;
    }
}
