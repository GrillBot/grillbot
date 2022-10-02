using GrillBot.App.Services.User.Points;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Actions.Api.V1.Points;

public class ServiceIncrementPoints : ApiAction
{
    private PointsService PointsService { get; }
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }

    public ServiceIncrementPoints(ApiRequestContext apiContext, PointsService pointsService, IDiscordClient discordClient, ITextsManager texts) : base(apiContext)
    {
        PointsService = pointsService;
        DiscordClient = discordClient;
        Texts = texts;
    }

    public async Task ProcessAsync(ulong guildId, ulong userId, int amount)
    {
        var user = await GetUserAsync(guildId, userId);
        await PointsService.IncrementPointsAsync(user, amount);
    }

    private async Task<IGuildUser> GetUserAsync(ulong guildId, ulong userId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId);
        var user = guild == null ? null : await guild.GetUserAsync(userId);

        return user ?? throw new NotFoundException(Texts["Points/Service/Increment/UserNotFound", ApiContext.Language]);
    }
}
