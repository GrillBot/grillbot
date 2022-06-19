using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class PermissionsRepository : RepositoryBase
{
    public PermissionsRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<ExplicitPermission>> GetAllowedPermissionsForCommand(string commandName)
    {
        using (Counter.Create("Database"))
        {
            return await Context.ExplicitPermissions.AsNoTracking()
                .Where(o => o.Command == commandName.Trim() && o.State == ExplicitPermissionState.Allowed)
                .ToListAsync();
        }
    }

    public async Task<bool> ExistsBannedCommandForUser(string commandName, IUser user)
    {
        using (Counter.Create("Database"))
        {
            return await Context.ExplicitPermissions.AsNoTracking()
                .AnyAsync(o => o.Command == commandName.Trim() && !o.IsRole && o.State == ExplicitPermissionState.Banned && o.TargetId == user.Id.ToString());
        }
    }

    public async Task<bool> ExistsCommandForTargetAsync(string command, string targetId)
    {
        using (Counter.Create("Database"))
        {
            return await Context.ExplicitPermissions.AsNoTracking()
                .AnyAsync(o => o.Command == command && o.TargetId == targetId);
        }
    }

    public async Task<ExplicitPermission?> FindPermissionForTargetAsync(string command, string targetId)
    {
        using (Counter.Create("Database"))
        {
            return await Context.ExplicitPermissions
                .FirstOrDefaultAsync(o => o.Command == command && o.TargetId == targetId);
        }
    }

    public async Task<List<ExplicitPermission>> GetPermissionsListAsync(string? commandQuery)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.ExplicitPermissions.AsNoTracking();
            if (!string.IsNullOrEmpty(commandQuery))
                query = query.Where(o => o.Command.Contains(commandQuery));

            return await query.ToListAsync();
        }
    }
}
