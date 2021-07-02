using GrillBot.App.Infrastructure.Embeds;
using System.Collections.Generic;

namespace GrillBot.App.Modules.Unverify
{
    public class UnverifyListMetadata : IEmbedMetadata
    {
        public string EmbedKind => "UnverifyList";

        public ulong GuildId { get; set; }
        public int Page { get; set; }

        public void SaveInto(IDictionary<string, string> destination)
        {
            destination[nameof(GuildId)] = GuildId.ToString();
            destination[nameof(Page)] = Page.ToString();
        }

        public bool TryLoadFrom(IReadOnlyDictionary<string, string> values)
        {
            ulong guildId = 0;
            int page = 0;

            var success = values.TryGetValue(nameof(GuildId), out var _guildId) && ulong.TryParse(_guildId, out guildId);
            success &= values.TryGetValue(nameof(Page), out var _page) && int.TryParse(_page, out page);

            if (success)
            {
                GuildId = guildId;
                Page = page;
                return true;
            }

            return false;
        }
    }
}
