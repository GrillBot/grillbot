using GrillBot.App.Infrastructure.Embeds;
using System.Collections.Generic;

namespace GrillBot.App.Modules.Channels
{
    public class ChannelboardMetadata : IEmbedMetadata
    {
        public string EmbedKind => "Channelboard";

        public int PageNumber { get; set; }
        public int TotalCount { get; set; }
        public ulong GuildId { get; set; }

        public void SaveInto(IDictionary<string, string> destination)
        {
            destination[nameof(PageNumber)] = PageNumber.ToString();
            destination[nameof(TotalCount)] = TotalCount.ToString();
            destination[nameof(GuildId)] = GuildId.ToString();
        }

        public bool TryLoadFrom(IReadOnlyDictionary<string, string> values)
        {
            int pageNumber = 0;
            int totalCount = 0;
            ulong guildId = 0;

            var success = values.TryGetValue(nameof(PageNumber), out string _pageNumber) && int.TryParse(_pageNumber, out pageNumber);
            success = success && values.TryGetValue(nameof(TotalCount), out string _totalCount) && int.TryParse(_totalCount, out totalCount);
            success = success && values.TryGetValue(nameof(GuildId), out string _guildId) && ulong.TryParse(_guildId, out guildId);

            if (success)
            {
                PageNumber = pageNumber;
                TotalCount = totalCount;
                GuildId = guildId;
                return true;
            }

            return false;
        }
    }
}
