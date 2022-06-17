using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Counters;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.Guilds;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Services.Permissions;

public class PermissionsCleaner
{
    private CounterManager Counter { get; }
    private ILogger<PermissionsCleaner> Logger { get; }

    public PermissionsCleaner(CounterManager counter, ILogger<PermissionsCleaner> logger)
    {
        Counter = counter;
        Logger = logger;
    }

    public static async Task<List<UselessPermission>> GetUselessPermissionsForUser(IGuildUser user, IGuild guild)
    {
        var channels = (await guild.GetChannelsAsync()).ToList();

        return channels
            .Where(o => o is not IThreadChannel)
            .Select(o => GetUselessPermission(o, user, guild))
            .Where(o => o != null)
            .ToList();
    }

    public static async Task<List<UselessPermission>> GetUselessPermissionsForChannelAsync(IGuildChannel channel, IGuild guild)
    {
        var users = await guild.GetUsersAsync();
        return users
            .Select(o => GetUselessPermission(channel, o, guild))
            .Where(o => o != null)
            .ToList();
    }

    private static UselessPermission GetUselessPermission(IGuildChannel channel, IGuildUser user, IGuild guild)
    {
        var overwrite = channel.GetPermissionOverwrite(user);
        if (overwrite == null) return null; // Overwrite not exists. Skip

        if (user.GuildPermissions.Administrator)
        {
            // User have Administrator permission. This user don't need some overwrites.
            return new UselessPermission(channel, user, UselessPermissionType.Administrator);
        }

        if (overwrite.Value.AllowValue == 0 && overwrite.Value.DenyValue == 0)
        {
            // Or user have neutral overwrite (overwrite without permissions).
            return new UselessPermission(channel, user, UselessPermissionType.Neutral);
        }

        foreach (var role in user.GetRoles().OrderByDescending(o => o.Position))
        {
            var roleOverwrite = channel.GetPermissionOverwrite(role);
            if (roleOverwrite == null) continue;

            // User have something extra.
            if (roleOverwrite.Value.AllowValue != overwrite.Value.AllowValue || roleOverwrite.Value.DenyValue != overwrite.Value.DenyValue)
                break;

            return new UselessPermission(channel, user, UselessPermissionType.AvailableFromRole);
        }

        return null;
    }

    public async Task RemoveUselessPermissionAsync(UselessPermission permission)
    {
        using (Counter.Create("Discord.API"))
        {
            Logger.LogInformation("Removing useless permission for user {Username}#{Discriminator} ({Type}, #{Name})", permission.User.Username,
                permission.User.Discriminator, permission.Type, permission.Channel.Name);
            await permission.Channel.RemovePermissionOverwriteAsync(permission.User);
        }
    }
}
