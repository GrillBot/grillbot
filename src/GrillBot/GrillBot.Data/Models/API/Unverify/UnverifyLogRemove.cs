using Discord.WebSocket;
using System.Collections.Generic;

namespace GrillBot.Data.Models.API.Unverify
{
    public class UnverifyLogRemove
    {
        public List<Role> ReturnedRoles { get; set; }
        public List<string> ReturnedChannelIds { get; set; }

        public UnverifyLogRemove() { }

        public UnverifyLogRemove(Models.Unverify.UnverifyLogRemove entity, SocketGuild guild)
        {
            ReturnedRoles = entity.ReturnedRoles.ConvertAll(o =>
            {
                var role = guild.GetRole(o);
                return role != null ? new Role(role) : null;
            });

            ReturnedChannelIds = entity.ReturnedOverwrites.ConvertAll(o => o.ChannelId.ToString());
        }
    }
}
