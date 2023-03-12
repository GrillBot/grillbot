using GrillBot.App.Helpers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Actions.Api.V1.Points;

public class ServiceTransferPoints : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private PointsHelper PointsHelper { get; }

    public ServiceTransferPoints(ApiRequestContext apiContext, IDiscordClient discordClient, ITextsManager texts, GrillBotDatabaseBuilder databaseBuilder, PointsHelper pointsHelper) : base(apiContext)
    {
        DiscordClient = discordClient;
        PointsHelper = pointsHelper;
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task ProcessAsync(ulong guildId, ulong fromUserId, ulong toUserId, int amount)
    {
        var (from, to) = await GetAndCheckUsersAsync(guildId, fromUserId, toUserId);

        await using var repository = DatabaseBuilder.CreateRepository();

        var fromUserPoints = await repository.Points.ComputePointsOfUserAsync(from.GuildId, from.Id);
        if (fromUserPoints < amount)
            throw new ValidationException(Texts["Points/Service/Transfer/InsufficientAmount", ApiContext.Language].FormatWith(from.GetFullName())).ToBadRequestValidation(amount, nameof(from));

        var fromGuildUser = await repository.GuildUser.FindGuildUserAsync(from);
        await repository.User.GetOrCreateUserAsync(to);
        await repository.Guild.GetOrCreateGuildAsync(to.Guild);
        var toGuildUser = await repository.GuildUser.GetOrCreateGuildUserAsync(to);

        var fromUserTransaction = PointsHelper.CreateTransaction(fromGuildUser!, null, 0, true)!;
        var toUserTransaction = PointsHelper.CreateTransaction(toGuildUser, null, 0, true)!;

        fromUserTransaction.Points = -amount;
        toUserTransaction.Points = amount;

        var transactions = await PointsHelper.FilterTransactionsAsync(repository, fromUserTransaction, toUserTransaction);
        await repository.AddCollectionAsync(transactions);
        await repository.CommitAsync();
    }

    private async Task<(IGuildUser from, IGuildUser to)> GetAndCheckUsersAsync(ulong guildId, ulong fromUserId, ulong toUserId)
    {
        if (fromUserId == toUserId)
            throw new ValidationException(Texts["Points/Service/Transfer/SameAccounts", ApiContext.Language]).ToBadRequestValidation($"{fromUserId}->{toUserId}", nameof(fromUserId), nameof(toUserId));

        var guild = await DiscordClient.GetGuildAsync(guildId);
        if (guild == null) throw new NotFoundException(Texts["Points/Service/Transfer/GuildNotFound", ApiContext.Language]);

        var fromUser = await guild.GetUserAsync(fromUserId);
        var toUser = await guild.GetUserAsync(toUserId);

        return (CheckUser(fromUser, true), CheckUser(toUser, false));
    }

    private IGuildUser CheckUser(IGuildUser user, bool isSource)
    {
        if (user == null)
            throw new NotFoundException(Texts[$"Points/Service/Transfer/{(isSource ? "SourceUserNotFound" : "DestUserNotFound")}", ApiContext.Language]);
        if (!user.IsUser())
            throw new ValidationException(Texts["Points/Service/Transfer/UserIsBot", ApiContext.Language].FormatWith(user.GetFullName())).ToBadRequestValidation(user.Id,
                isSource ? "fromUserId" : "toUserId");
        return user;
    }
}
