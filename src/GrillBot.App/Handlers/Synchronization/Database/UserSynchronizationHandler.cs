using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.Synchronization.Database;

public class UserSynchronizationHandler : BaseSynchronizationHandler, IUserJoinedEvent, IGuildMemberUpdatedEvent, IUserUpdatedEvent
{
    public UserSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
    }

    public async Task ProcessAsync(IGuildUser user)
    {
        await using var repository = CreateRepository();

        await repository.User.GetOrCreateUserAsync(user);
        await repository.CommitAsync();

        await repository.Guild.GetOrCreateGuildAsync(user.Guild);
        await repository.CommitAsync();

        await repository.GuildUser.GetOrCreateGuildUserAsync(user);
        await repository.CommitAsync();
    }

    public async Task ProcessAsync(IGuildUser? before, IGuildUser after)
    {
        if (before is null || (before.Nickname == after.Nickname && before.Username == after.Username && before.Discriminator == after.Discriminator))
            return;

        await using var repository = CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(after.Guild);
        await repository.CommitAsync();

        await repository.User.GetOrCreateUserAsync(after);
        await repository.CommitAsync();

        await repository.GuildUser.GetOrCreateGuildUserAsync(after);
        await repository.CommitAsync();
    }

    public async Task ProcessAsync(IUser before, IUser after)
    {
        if (before.Username == after.Username && before.Discriminator == after.Discriminator)
            return;

        await using var repository = CreateRepository();

        await repository.User.GetOrCreateUserAsync(after);
        await repository.CommitAsync();
    }
}
