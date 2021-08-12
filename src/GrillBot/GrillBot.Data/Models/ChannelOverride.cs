using Discord;
using Newtonsoft.Json;

namespace GrillBot.Data.Models
{
    /// <summary>
    /// Channel override
    /// </summary>
    public class ChannelOverride
    {
        /// <summary>
        /// Channel ID
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// Allow permissions value
        /// </summary>
        public ulong AllowValue { get; set; }

        /// <summary>
        /// Deny permissions value
        /// </summary>
        public ulong DenyValue { get; set; }

        [JsonIgnore]
        public OverwritePermissions Permissions => new(AllowValue, DenyValue);

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
