using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Database.Enums;

namespace GrillBot.App.Managers;

public class UserManager
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UserManager(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task SetHearthbeatAsync(bool isActive, ApiRequestContext context)
    {
        var isPublic = context.IsPublic();

        await using var repository = DatabaseBuilder.CreateRepository();
        var user = await repository.User.FindUserAsync(context.LoggedUser!);
        if (user == null)
            throw new NotFoundException();

        if (isActive)
            user.Flags |= (int)(isPublic ? UserFlags.PublicAdminOnline : UserFlags.WebAdminOnline);
        else
            user.Flags &= ~(int)(isPublic ? UserFlags.PublicAdminOnline : UserFlags.WebAdminOnline);

        await repository.CommitAsync();
    }

    public async Task<bool> CheckFlagsAsync(IUser user, UserFlags flags)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.User.FindUserAsync(user, true);
        return entity?.HaveFlags(flags) == true;
    }
}
