using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Services.User.Points;

public partial class PointsService
{
    public async Task IncrementPointsAsync(IGuildUser toUser, int amount)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(toUser.Guild);
        await repository.User.GetOrCreateUserAsync(toUser);
        var guildUserEntity = await repository.GuildUser.GetOrCreateGuildUserAsync(toUser);

        var transaction = CreateTransaction(guildUserEntity, null, 0, true);
        transaction.Points = amount;

        await repository.AddAsync(transaction);
        await repository.CommitAsync();
        await RecalculatePointsSummaryAsync(repository, true, new List<IGuildUser> { toUser });
    }

    public async Task TransferPointsAsync(IGuildUser fromUser, IGuildUser toUser, int amount, string locale)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var fromUserPoints = await repository.Points.ComputePointsOfUserAsync(fromUser.GuildId, fromUser.Id);
        if (fromUserPoints < amount)
            throw new ValidationException(Texts["Points/Service/Transfer/InsufficientAmount", locale].FormatWith(fromUser.GetFullName())).ToBadRequestValidation(amount, nameof(fromUser));

        var fromGuildUser = await repository.GuildUser.FindGuildUserAsync(fromUser);
        await repository.User.GetOrCreateUserAsync(toUser);
        await repository.Guild.GetOrCreateGuildAsync(toUser.Guild);
        var toGuildUser = await repository.GuildUser.GetOrCreateGuildUserAsync(toUser);

        var fromUserTransaction = CreateTransaction(fromGuildUser, null, 0, true);
        fromUserTransaction.Points = -amount;
        await repository.AddAsync(fromUserTransaction);

        var toUserTransaction = CreateTransaction(toGuildUser, null, 0, true);
        toUserTransaction.Points = amount;
        await repository.AddAsync(toUserTransaction);

        await repository.CommitAsync();
        await RecalculatePointsSummaryAsync(repository, true, new List<IGuildUser> { fromUser, toUser });
    }
}
