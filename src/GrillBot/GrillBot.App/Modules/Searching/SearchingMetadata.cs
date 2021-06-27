    using GrillBot.App.Infrastructure.Embeds;
using System.Collections.Generic;

namespace GrillBot.App.Modules.Searching
{
    public class SearchingMetadata : IEmbedMetadata
    {
        public string EmbedKind => "Search";

        public int Page { get; set; }
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }

        public void SaveInto(IDictionary<string, string> destination)
        {
            destination[nameof(Page)] = Page.ToString();
            destination[nameof(ChannelId)] = ChannelId.ToString();
            destination[nameof(GuildId)] = GuildId.ToString();
        }

        public bool TryLoadFrom(IReadOnlyDictionary<string, string> values)
        {
            int page = 0;
            ulong guildId = 0;
            ulong channelId = 0;

            var success = values.TryGetValue(nameof(Page), out var _page) && int.TryParse(_page, out page);
            success &= values.TryGetValue(nameof(GuildId), out var _guildId) && ulong.TryParse(_guildId, out guildId);
            success &= values.TryGetValue(nameof(ChannelId), out var _channelId) && ulong.TryParse(_channelId, out channelId);

            if (success)
            {
                Page = page;
                GuildId = guildId;
                ChannelId = channelId;
                return true;
            }

            return false;
        }
    }
}
