using GrillBot.Database.Enums;
using GrillBot.Database.Extensions;

namespace GrillBot.App.Services.Discord.Synchronization;

public class UserSynchronization : SynchronizationBase
{
    public UserSynchronization(GrillBotDatabaseBuilder dbFactory) : base(dbFactory)
    {
    }

    public async Task UserUpdatedAsync(IUser _, IUser after)
    {
        using var context = DbFactory.Create();

        var user = await context.Users.FirstOrDefaultAsync(o => o.Id == after.Id.ToString());
        if (user == null) return;

        user.Username = after.Username;
        user.Discriminator = after.Discriminator;
        user.Status = after.GetStatus();

        if (!after.IsUser())
            user.Flags |= (int)UserFlags.NotUser;

        await context.SaveChangesAsync();
    }

    public async Task InitBotAdminAsync(GrillBotContext context, IApplication application)
    {
        var botOwner = await context.Users.FirstOrDefaultAsync(o => o.Id == application.Owner.Id.ToString());
        if (botOwner == null)
        {
            botOwner = Database.Entity.User.FromDiscord(application.Owner);
            await context.AddAsync(botOwner);
        }

        botOwner.Flags |= (int)UserFlags.BotAdmin;
        botOwner.Flags &= ~(int)UserFlags.NotUser;
        botOwner.Username = application.Owner.Username;
        botOwner.Discriminator = application.Owner.Discriminator;
    }

    public async Task PresenceUpdatedAsync(IUser user, SocketPresence _, SocketPresence after)
    {
        using var context = DbFactory.Create();

        var dbUser = await context.Users.FirstOrDefaultAsync(o => o.Id == user.Id.ToString());
        if (dbUser == null) return;

        dbUser.Username = user.Username;
        dbUser.Discriminator = user.Discriminator;
        dbUser.Status = after.GetStatus();

        if (!user.IsUser())
            dbUser.Flags |= (int)UserFlags.NotUser;

        await context.SaveChangesAsync();
    }
}
