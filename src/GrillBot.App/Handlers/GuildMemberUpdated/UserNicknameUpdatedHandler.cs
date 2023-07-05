using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Handlers.GuildMemberUpdated;

public class UserNicknameUpdatedHandler : IGuildMemberUpdatedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UserNicknameUpdatedHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IGuildUser? before, IGuildUser after)
    {
        if (!CanProcess(before, after)) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(after.Guild);
        await repository.User.GetOrCreateUserAsync(after);
        await repository.GuildUser.GetOrCreateGuildUserAsync(after);

        var canSave = await CreateEntityAsync(repository, before!) || await CreateEntityAsync(repository, after);
        if (canSave)
            await repository.CommitAsync();
    }

    private static bool CanProcess(IGuildUser? before, IGuildUser after)
    {
        if (before == null) return false;
        if (string.IsNullOrEmpty(before.Nickname) && string.IsNullOrEmpty(after.Nickname)) return false;

        return before.Nickname != after.Nickname;
    }

    private static async Task<bool> CreateEntityAsync(GrillBotRepository repository, IGuildUser user)
    {
        if (string.IsNullOrEmpty(user.Nickname) || await repository.Nickname.ExistsAsync(user))
            return false;

        var entity = new Nickname
        {
            GuildId = user.GuildId.ToString(),
            NicknameValue = user.Nickname,
            UserId = user.Id.ToString(),
            Id = await repository.Nickname.ComputeIdAsync(user)
        };

        await repository.AddAsync(entity);
        return true;
    }
}
