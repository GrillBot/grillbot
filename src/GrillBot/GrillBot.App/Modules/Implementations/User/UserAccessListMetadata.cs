using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.User;

public class UserAccessListMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "UserAccessList";

    public ulong ForUserId { get; set; }
    public ulong GuildId { get; set; }

    protected override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(ForUserId)] = ForUserId.ToString();
        destination[nameof(GuildId)] = GuildId.ToString();
    }

    protected override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong forUserId = 0;
        ulong guildId = 0;

        var success = values.TryGetValue(nameof(ForUserId), out var forUserIdData) && ulong.TryParse(forUserIdData, out forUserId);
        success &= values.TryGetValue(nameof(GuildId), out var guildIdData) && ulong.TryParse(guildIdData, out guildId);

        if (!success)
            return false;

        ForUserId = forUserId;
        GuildId = guildId;
        return true;
    }

    protected override void Reset()
    {
        ForUserId = default;
        GuildId = default;
    }
}
