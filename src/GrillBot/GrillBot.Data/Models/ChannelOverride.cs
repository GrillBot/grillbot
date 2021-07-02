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

        public ChannelOverride(ulong channelId, ulong allowValue, ulong denyValue)
        {
            AllowValue = allowValue;
            DenyValue = denyValue;
            ChannelId = channelId;
        }

        public ChannelOverride(IChannel channel, OverwritePermissions perms) : this(channel.Id, perms.AllowValue, perms.DenyValue)
        {
        }
    }
}
