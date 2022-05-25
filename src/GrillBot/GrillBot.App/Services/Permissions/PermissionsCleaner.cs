using GrillBot.App.Infrastructure;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.Guilds;

namespace GrillBot.App.Services.Permissions;

public class PermissionsCleaner : ServiceBase
{
    public PermissionsCleaner(IDiscordClient client) : base(null, null, client, null)
    {
    }

    public async Task<List<UselessPermission>> GetUselessPermissionsForUser(IGuildUser user, IGuild guild)
    {
        var result = new List<UselessPermission>();
        var channels = (await guild.GetChannelsAsync()).ToList();
        foreach (var channel in channels.Where(o => o is not SocketThreadChannel))
        {
            var overwrite = channel.GetPermissionOverwrite(user);
            if (overwrite == null) continue; // Overwrite not exists. Skip

            if (user.GuildPermissions.Administrator)
            {
                // User have Administrator permission. This user don't need some overwrites.
                result.Add(new(channel, user, UselessPermissionType.Administrator));
                continue;
            }

            if (overwrite.Value.AllowValue == 0 && overwrite.Value.DenyValue == 0)
            {
                // Or user have neutral overwrite (overwrite without permissions).
                result.Add(new UselessPermission(channel, user, UselessPermissionType.Neutral));
                continue;
            }

            foreach (var role in user.RoleIds.Select(o => guild.GetRole(o)).Where(o => o != null).OrderByDescending(o => o.Position))
            {
                var roleOverwrite = channel.GetPermissionOverwrite(role);
                if (roleOverwrite == null) continue;

                // User have something extra.
                if (roleOverwrite.Value.AllowValue != overwrite.Value.AllowValue || roleOverwrite.Value.DenyValue != overwrite.Value.DenyValue)
                    break;

                result.Add(new UselessPermission(channel, user, UselessPermissionType.AvailableFromRole));
                break;
            }
        }

        return result;
    }
}
