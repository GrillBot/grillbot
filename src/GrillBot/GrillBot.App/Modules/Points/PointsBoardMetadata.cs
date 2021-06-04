using GrillBot.App.Infrastructure.Embeds;
using System.Collections.Generic;

namespace GrillBot.App.Modules.Points
{
    public class PointsBoardMetadata : IEmbedMetadata
    {
        public string EmbedKind => "Points";

        public int PageNumber { get; set; }
        public ulong GuildId { get; set; }

        public void SaveInto(IDictionary<string, string> destination)
        {
            destination[nameof(PageNumber)] = PageNumber.ToString();
            destination[nameof(GuildId)] = GuildId.ToString();
        }

        public bool TryLoadFrom(IReadOnlyDictionary<string, string> values)
        {
            int pageNumber = 0;
            ulong guildId = 0;

            var success = values.TryGetValue(nameof(PageNumber), out string _pageNumber) && int.TryParse(_pageNumber, out pageNumber);
            success = success && values.TryGetValue(nameof(GuildId), out string _guildId) && ulong.TryParse(_guildId, out guildId);

            if (success)
            {
                PageNumber = pageNumber;
                GuildId = guildId;
                return true;
            }

            return false;
        }
    }
}
