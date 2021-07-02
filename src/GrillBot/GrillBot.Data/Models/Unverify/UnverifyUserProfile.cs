using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Data.Models.Unverify
{
    public class UnverifyUserProfile
    {
        public IGuildUser Destination { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<SocketRole> RolesToRemove { get; set; }
        public List<SocketRole> RolesToKeep { get; set; }
        public List<ChannelOverride> ChannelsToKeep { get; set; }
        public List<ChannelOverride> ChannelsToRemove { get; set; }
        public string Reason { get; set; }
        public bool IsSelfUnverify { get; set; }

        public UnverifyUserProfile(IGuildUser destination, DateTime start, DateTime end, bool isSelfUnverify)
        {
            Destination = destination;
            Start = start;
            End = end;
            IsSelfUnverify = isSelfUnverify;

            RolesToKeep = new List<SocketRole>();
            RolesToRemove = new List<SocketRole>();
            ChannelsToKeep = new List<ChannelOverride>();
            ChannelsToRemove = new List<ChannelOverride>();
        }

        public Task ReturnRolesAsync() => Destination.AddRolesAsync(RolesToRemove);

        public async Task ReturnChannelsAsync(SocketGuild guild)
        {
            var channels = ChannelsToRemove
                .Select(o => new { Channel = guild.GetChannel(o.ChannelId), Perms = o.Permissions })
                .Where(o => o.Channel != null);

            foreach (var channel in channels)
            {
                await channel.Channel.AddPermissionOverwriteAsync(Destination, channel.Perms);
            }
        }

        public Task RemoveRolesAsync() => Destination.RemoveRolesAsync(RolesToRemove);

        public async Task RemoveChannelsAsync(SocketGuild guild)
        {
            var channels = ChannelsToRemove
                .Select(o => new { Channel = guild.GetChannel(o.ChannelId), Perms = o.Permissions })
                .Where(o => o.Channel != null);

            foreach (var channel in channels)
            {
                await channel.Channel.RemovePermissionOverwriteAsync(Destination);
            }
        }
    }
}
