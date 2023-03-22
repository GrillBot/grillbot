using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;

namespace GrillBot.App.Actions.Api.V1.Points;

public class ServiceIncrementPoints : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private PointsHelper PointsHelper { get; }

    public ServiceIncrementPoints(ApiRequestContext apiContext, IDiscordClient discordClient, ITextsManager texts, GrillBotDatabaseBuilder databaseBuilder, PointsHelper pointsHelper) :
        base(apiContext)
    {
        DiscordClient = discordClient;
        Texts = texts;
        DatabaseBuilder = databaseBuilder;
        PointsHelper = pointsHelper;
    }

    public async Task ProcessAsync(ulong guildId, ulong userId, int amount)
    {
        var user = await GetUserAsync(guildId, userId);

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(user.Guild);
        await repository.User.GetOrCreateUserAsync(user);

        var userEntity = await repository.GuildUser.GetOrCreateGuildUserAsync(user);
        var transaction = PointsHelper.CreateTransaction(userEntity, null, 0, true);
        transaction.Points = amount;

        await repository.AddAsync(transaction);
        await repository.CommitAsync();
    }

    private async Task<IGuildUser> GetUserAsync(ulong guildId, ulong userId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId);
        var user = guild == null ? null : await guild.GetUserAsync(userId);

        return user ?? throw new NotFoundException(Texts["Points/Service/Increment/UserNotFound", ApiContext.Language]);
    }
}
