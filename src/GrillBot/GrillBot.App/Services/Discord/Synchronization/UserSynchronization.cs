using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Discord.Synchronization;

public class UserSynchronization : SynchronizationBase
{
    public UserSynchronization(GrillBotContextFactory dbFactory) : base(dbFactory)
    {
    }

    public async Task UserUpdatedAsync(IUser _, IUser after)
    {
        using var context = DbFactory.Create();

        var user = await context.Users.FirstOrDefaultAsync(o => o.Id == after.Id.ToString());
        if (user == null) return;

        user.Username = after.Username;
        user.Discriminator = after.Discriminator;

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
}
