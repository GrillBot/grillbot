using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.Guilds;

namespace GrillBot.App.Actions.Commands;

public class PermissionsReader : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public PermissionsReader(GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task<List<UselessPermission>> ReadUselessPermissionsAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var unverifyUserIds = await repository.Unverify.GetUserIdsWithUnverify(Context.Guild);

        var result = new List<UselessPermission>();
        var guildUsers = await Context.Guild.GetUsersAsync();
        foreach (var user in guildUsers.Where(o => !unverifyUserIds.Contains(o.Id))) // Ignore users with unverify.
            result.AddRange(await ReadUselessPermissionsForUserAsync(user));
        return result;
    }

    public async Task<List<UselessPermission>> ReadUselessPermissionsForUserAsync(IGuildUser user)
    {
        var channels = await Context.Guild.GetChannelsAsync();

        return channels
            .Where(o => o is not IThreadChannel)
            .Select(o => GetUselessPermission(o, user))
            .Where(o => o != null)
            .ToList();
    }

    private static UselessPermission GetUselessPermission(IGuildChannel channel, IGuildUser user)
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

        // If the user is not an administrator and at the same time this overwrite is not neutral, then
        // must be cross-checked with the roles defined for the channel and the roles the user has.

        // From these roles, which are defined for the channel and the user has them, an aggregate permission will be made.
        // After that, it goes through the aggregated roles and looks for some reason why the permission cannot be removed.
        // If the user is found to have some additional permission that cannot get from the role, then the entire result is evaluated as the permission cannot be deleted.

        var roleOverwrites = user.GetRoles().OrderByDescending(o => o.Position).Select(channel.GetPermissionOverwrite).Where(o => o != null).Select(o => o.Value).ToList();
        var rolePermissions = new OverwritePermissions(
            roleOverwrites.Aggregate(0UL, (prev, current) => prev | current.AllowValue), // Create aggregated AllowValue.
            roleOverwrites.Aggregate(0UL, (prev, current) => prev | current.DenyValue) // Create aggregated DenyValue.
        );

        var (roleAllowList, roleDenyList) = (rolePermissions.ToAllowList(), rolePermissions.ToDenyList());

        var canDelete = overwrite.Value.ToAllowList().Aggregate(true, (prev, current) => prev && roleAllowList.Exists(o => o == current));
        canDelete = overwrite.Value.ToDenyList().Aggregate(canDelete, (prev, current) => prev && roleDenyList.Exists(o => o == current));
        return canDelete ? new UselessPermission(channel, user, UselessPermissionType.AvailableFromRole) : null;
    }

    public string CreateSummary(List<UselessPermission> uselessPermissions)
    {
        var channelsCount = uselessPermissions.Select(o => o.Channel.Id).Distinct().Count();
        var usersCount = uselessPermissions.Select(o => o.User.Id).Count();

        return Texts["Permissions/Useless/CheckSummary", Locale].FormatWith(uselessPermissions.Count, channelsCount, usersCount);
    }
}
