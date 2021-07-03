using GrillBot.App.Infrastructure.Embeds;
using System.Collections.Generic;

namespace GrillBot.App.Modules.Points
{
    public class PointsBoardMetadata : PaginatedMetadataBase
    {
        public override string EmbedKind => "Points";

        public ulong GuildId { get; set; }

        public override void Save(IDictionary<string, string> destination)
        {
            destination[nameof(GuildId)] = GuildId.ToString();
        }

        public override bool TryLoad(IReadOnlyDictionary<string, string> values)
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

        public override void Reset()
        {
            GuildId = default;
        }
    }
}
