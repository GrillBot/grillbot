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
}
