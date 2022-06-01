using GrillBot.App.Infrastructure;
using GrillBot.Common.Managers.Counters;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.Guilds;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Services.Permissions;

public class PermissionsCleaner
{
    private IDiscordClient DiscordClient { get; }
    private CounterManager Counter { get; }
    private ILogger<PermissionsCleaner> Logger { get; }

    public PermissionsCleaner(IDiscordClient client, CounterManager counter,
        ILogger<PermissionsCleaner> logger)
    {
        DiscordClient = client;
        Counter = counter;
        Logger = logger;
    }

    public async Task<List<UselessPermission>> GetUselessPermissionsForUser(IGuildUser user, IGuild guild)
    {
        var result = new List<UselessPermission>();
        var channels = (await guild.GetChannelsAsync()).ToList();

        foreach (var channel in channels.Where(o => o is not SocketThreadChannel))
        {
            var permission = await GetUselessPermissionAsync(channel, user, guild);
            if (permission != null)
                result.Add(permission);
        }

        return result;
    }

    public async Task<List<UselessPermission>> GetUselessPermissionsForChannelAsync(IGuildChannel channel, IGuild guild)
    {
        var result = new List<UselessPermission>();
        var users = await guild.GetUsersAsync();

        foreach (var user in users)
        {
            var permission = await GetUselessPermissionAsync(channel, user, guild);
            if (permission != null)
                result.Add(permission);
        }

        return result;
    }

    private async Task<UselessPermission> GetUselessPermissionAsync(IGuildChannel channel, IGuildUser user, IGuild guild)
    {
        var overwrite = channel.GetPermissionOverwrite(user);
        if (overwrite == null) return null; // Overwrite not exists. Skip

        if (user.GuildPermissions.Administrator)
        {
            // User have Administrator permission. This user don't need some overwrites.
            return new(channel, user, UselessPermissionType.Administrator);
        }

        if (overwrite.Value.AllowValue == 0 && overwrite.Value.DenyValue == 0)
        {
            // Or user have neutral overwrite (overwrite without permissions).
            return new UselessPermission(channel, user, UselessPermissionType.Neutral);
        }

        foreach (var role in user.RoleIds.Select(o => guild.GetRole(o)).Where(o => o != null).OrderByDescending(o => o.Position))
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
