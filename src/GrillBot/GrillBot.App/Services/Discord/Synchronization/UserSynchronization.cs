using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Services.Discord.Synchronization;

public class UserSynchronization : SynchronizationBase
{
    public UserSynchronization(GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
    }

    public async Task UserUpdatedAsync(IUser after)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var user = await repository.User.FindUserAsync(after);
        if (user == null) return;

        await repository.CommitAsync();
    }

    public static async Task InitBotAdminAsync(GrillBotRepository repository, IApplication application)
    {
        var botOwner = await repository.User.GetOrCreateUserAsync(application.Owner);
        botOwner.Flags |= (int)UserFlags.BotAdmin;
        botOwner.Flags &= ~(int)UserFlags.NotUser;
    }

    public async Task PresenceUpdatedAsync(IUser user, SocketPresence after)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        await repository.User.UpdateStatusAsync(user.Id, after.GetStatus());
    }
}
