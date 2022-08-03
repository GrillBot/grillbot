using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Services.User.Points;

public partial class PointsService
{
    public async Task IncrementPointsAsync(IGuildUser toUser, int amount)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateRepositoryAsync(toUser.Guild);
        await repository.User.GetOrCreateUserAsync(toUser);
        var guildUserEntity = await repository.GuildUser.GetOrCreateGuildUserAsync(toUser);

        var transaction = CreateTransaction(guildUserEntity, false, 0, true);
        transaction.Points = amount;

        await repository.AddAsync(transaction);
        await repository.CommitAsync();
        await RecalculatePointsSummaryAsync(repository, true, new List<IGuildUser> { toUser });
    }

    public async Task TransferPointsAsync(IGuildUser fromUser, IGuildUser toUser, int amount)
    {
        if (fromUser.Id == toUser.Id)
            throw new InvalidOperationException("Nelze převést body mezi stejnými účty.");

        if (!fromUser.IsUser())
            throw new InvalidOperationException($"Nelze převést body od `{fromUser.GetDisplayName()}`, protože se nejedná o běžného uživatele.");

        if (!toUser.IsUser())
            throw new InvalidOperationException($"Nelze převést body uživateli `{toUser.GetDisplayName()}`, protože se nejedná o běžného uživatele.");

        await using var repository = DatabaseBuilder.CreateRepository();

        var fromGuildUser = await repository.GuildUser.FindGuildUserAsync(fromUser);
        if (fromGuildUser == null)
            throw new InvalidOperationException($"Nelze převést body od uživatele `{fromUser.GetDisplayName()}`, protože žádné body ještě nemá.");

        var fromUserPoints = await repository.Points.ComputePointsOfUserAsync(fromUser.GuildId, fromUser.Id);
        if (fromUserPoints < amount)
            throw new InvalidOperationException($"Nelze převést body od uživatele `{fromUser.GetDisplayName()}`, protože jich nemá dostatek.");

        await repository.User.GetOrCreateUserAsync(toUser);
        await repository.Guild.GetOrCreateRepositoryAsync(toUser.Guild);
        var toGuildUser = await repository.GuildUser.GetOrCreateGuildUserAsync(toUser);

        var fromUserTransaction = CreateTransaction(fromGuildUser, false, 0, true);
        fromUserTransaction.Points = -amount;
        await repository.AddAsync(fromUserTransaction);

        var toUserTransaction = CreateTransaction(toGuildUser, false, 0, true);
        toUserTransaction.Points = amount;
        await repository.AddAsync(toUserTransaction);

        await repository.CommitAsync();
        await RecalculatePointsSummaryAsync(repository, true, new List<IGuildUser> { fromUser, toUser });
    }
}
