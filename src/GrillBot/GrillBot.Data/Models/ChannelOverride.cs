using Discord;
using Newtonsoft.Json;

namespace GrillBot.Data.Models
{
    public class ChannelOverride
    {
        public ulong ChannelId { get; set; }
        public ulong AllowValue { get; set; }
        public ulong DenyValue { get; set; }

        [JsonIgnore]
        public OverwritePermissions Permissions => new OverwritePermissions(AllowValue, DenyValue);

        public ChannelOverride() { }

        public ChannelOverride(IChannel channel, OverwritePermissions perms)
        {
            AllowValue = perms.AllowValue;
            DenyValue = perms.DenyValue;
            ChannelId = channel.Id;
        }
    }
}
