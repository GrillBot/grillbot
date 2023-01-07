using GrillBot.Database.Enums;

namespace GrillBot.App.Services.User;

public class UserService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UserService(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<bool> CheckUserFlagsAsync(IUser user, UserFlags flags)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var userEntity = await repository.User.FindUserAsync(user);
        return userEntity?.HaveFlags(flags) ?? false;
    }
}
