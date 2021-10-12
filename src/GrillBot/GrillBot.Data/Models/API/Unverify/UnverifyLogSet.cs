using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.API.Unverify
{
    public class UnverifyLogSet
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<Role> RolesToKeep { get; set; }
        public List<Role> RolesToRemove { get; set; }
        public List<string> ChannelIdsToKeep { get; set; }
        public List<string> ChannelIdsToRemove { get; set; }
        public string Reason { get; set; }
        public bool IsSelfUnverify { get; set; }

        public UnverifyLogSet() { }

        public UnverifyLogSet(Models.Unverify.UnverifyLogSet entity, SocketGuild guild)
        {
            Start = entity.Start;
            End = entity.End;

            RolesToKeep = entity.RolesToKeep.ConvertAll(o =>
            {
                var role = guild.GetRole(o);
                return role != null ? new Role(role) : null;
            });

            RolesToRemove = entity.RolesToRemove.ConvertAll(o =>
            {
                var role = guild.GetRole(o);
                return role != null ? new Role(role) : null;
            });

            ChannelIdsToKeep = entity.ChannelsToKeep.ConvertAll(o => o.ChannelId.ToString());
            ChannelIdsToRemove = entity.ChannelsToRemove.ConvertAll(o => o.ChannelId.ToString());
            Reason = entity.Reason;
            IsSelfUnverify = entity.IsSelfUnverify;
        }
    }
}
