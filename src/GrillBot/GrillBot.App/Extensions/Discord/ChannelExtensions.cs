using Discord;
using Discord.WebSocket;
using System.Linq;

namespace GrillBot.App.Extensions.Discord
{
    static public class ChannelExtensions
    {
        static public string GetMention(this IChannel channel) => $"<#{channel.Id}>";

        static public bool HaveAccess(this SocketGuildChannel channel, SocketGuildUser user)
        {
            if (channel.GetUser(user.Id) != null || channel.PermissionOverwrites.Count == 0)
                return true;

            var overwrite = channel.GetPermissionOverwrite(user);
            if (overwrite != null)
            {
                if (overwrite.Value.ViewChannel == PermValue.Allow)
                    return true;
                else if (overwrite.Value.ViewChannel == PermValue.Deny)
                    return false;
            }

            var everyonePerm = channel.GetPermissionOverwrite(user.Guild.EveryoneRole);
            var isEveryonePerm = everyonePerm != null && (everyonePerm.Value.ViewChannel == PermValue.Allow || everyonePerm.Value.ViewChannel == PermValue.Inherit);

            foreach (var role in user.Roles.Where(o => !o.IsEveryone))
            {
                var roleOverwrite = channel.GetPermissionOverwrite(role);
                if (roleOverwrite == null) continue;

                if (roleOverwrite.Value.ViewChannel == PermValue.Deny && isEveryonePerm)
                    return false;

                if (roleOverwrite.Value.ViewChannel == PermValue.Allow)
                    return true;
            }

            return isEveryonePerm;
        }
    }
}
