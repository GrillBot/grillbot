﻿using GrillBot.Common.Models;
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

        using var repository = DatabaseBuilder.CreateRepository();
        var user = await repository.User.FindUserAsync(context.LoggedUser!)
            ?? throw new NotFoundException();

        if (isActive)
            user.Flags |= (int)(isPublic ? UserFlags.PublicAdminOnline : UserFlags.WebAdminOnline);
        else
            user.Flags &= ~(int)(isPublic ? UserFlags.PublicAdminOnline : UserFlags.WebAdminOnline);

        await repository.CommitAsync();
    }

    public async Task<bool> CheckFlagsAsync(IUser user, UserFlags flags)
    {
        using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.User.FindUserAsync(user, true);
        return entity?.HaveFlags(flags) == true;
    }

    public async Task<string?> GetUserLanguage(IUser user)
    {
        using var repository = DatabaseBuilder.CreateRepository();
        var userEntity = await repository.User.FindUserByIdAsync(user.Id, disableTracking: true);

        return userEntity?.Language;
    }
}
