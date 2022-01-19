using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.User;

public class UserAccessListMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "UserAccessList";

    public ulong ForUserId { get; set; }
    public ulong GuildId { get; set; }

    public override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(ForUserId)] = ForUserId.ToString();
        destination[nameof(GuildId)] = GuildId.ToString();
    }

    public override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong forUserId = 0;
        ulong guildId = 0;

        var success = values.TryGetValue(nameof(ForUserId), out var _forUserId) && ulong.TryParse(_forUserId, out forUserId);
        success &= values.TryGetValue(nameof(GuildId), out var _guildId) && ulong.TryParse(_guildId, out guildId);

        if (success)
        {
            ForUserId = forUserId;
            GuildId = guildId;
            return true;
        }

        return false;
    }

    public override void Reset()
    {
        ForUserId = default;
        GuildId = default;
    }
}
