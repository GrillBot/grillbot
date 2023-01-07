using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Enums;

namespace GrillBot.App.Managers;

public class HearthbeatManager
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public HearthbeatManager(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task SetAsync(bool isActive, ApiRequestContext context)
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
}
