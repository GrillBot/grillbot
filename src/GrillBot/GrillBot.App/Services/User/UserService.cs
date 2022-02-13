using GrillBot.Database.Enums;

namespace GrillBot.App.Services.User;

public class UserService
{
    private GrillBotContextFactory DbFactory { get; }

    public UserService(GrillBotContextFactory dbFactory)
    {
        DbFactory = dbFactory;
    }

    public async Task<bool> IsUserBotAdminAsync(IUser user)
    {
        using var context = DbFactory.Create();

        var dbUser = await context.Users.FirstOrDefaultAsync(o => o.Id == user.Id.ToString());
        return dbUser?.HaveFlags(UserFlags.BotAdmin) ?? false;
    }
}
