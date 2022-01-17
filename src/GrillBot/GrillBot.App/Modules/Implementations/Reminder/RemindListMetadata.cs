using GrillBot.Data.Infrastructure.Embeds;
using System.Collections.Generic;

namespace GrillBot.Data.Modules.Implementations.Reminder;

public class RemindListMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Reminder";

    public ulong OfUser { get; set; }

    public override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(OfUser)] = OfUser.ToString();
    }

    public override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong ofUser = 0;
        var success = values.TryGetValue(nameof(OfUser), out var _ofUser) && ulong.TryParse(_ofUser, out ofUser);

        if (success)
        {
            OfUser = ofUser;
            return true;
        }

        return false;
    }

    public override void Reset()
    {
        OfUser = default;
    }
}
