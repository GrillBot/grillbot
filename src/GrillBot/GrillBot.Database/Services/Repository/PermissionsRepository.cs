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
}
