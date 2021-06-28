using GrillBot.App.Infrastructure.Embeds;
using System.Collections.Generic;

namespace GrillBot.App.Modules.User
{
    public class UserAccessListMetadata : IEmbedMetadata
    {
        public string EmbedKind => "UserAccessList";

        public ulong ForUserId { get; set; }
        public ulong GuildId { get; set; }
        public int Page { get; set; }

        public void SaveInto(IDictionary<string, string> destination)
        {
            destination[nameof(ForUserId)] = ForUserId.ToString();
            destination[nameof(GuildId)] = GuildId.ToString();
            destination[nameof(Page)] = Page.ToString();
        }

        public bool TryLoadFrom(IReadOnlyDictionary<string, string> values)
        {
            ulong forUserId = 0;
            ulong guildId = 0;
            int page = 0;

            var success = values.TryGetValue(nameof(ForUserId), out var _forUserId) && ulong.TryParse(_forUserId, out forUserId);
            success &= values.TryGetValue(nameof(GuildId), out var _guildId) && ulong.TryParse(_guildId, out guildId);
            success &= values.TryGetValue(nameof(Page), out var _page) && int.TryParse(_page, out page);

            if (success)
            {
                ForUserId = forUserId;
                GuildId = guildId;
                Page = page;
                return true;
            }

            return false;
        }
    }
}
