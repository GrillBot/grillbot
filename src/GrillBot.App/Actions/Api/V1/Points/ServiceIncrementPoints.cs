using GrillBot.App.Managers.Points;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.PointsService.Models.Events;

namespace GrillBot.App.Actions.Api.V1.Points;

public class ServiceIncrementPoints : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }

    private IGuild? Guild { get; set; }
    private IUser? User { get; set; }

    private readonly PointsManager _pointsManager;

    public ServiceIncrementPoints(ApiRequestContext apiContext, IDiscordClient discordClient, ITextsManager texts, PointsManager pointsManager) : base(apiContext)
    {
        DiscordClient = discordClient;
        Texts = texts;
        _pointsManager = pointsManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (ulong)Parameters[0]!;
        var userId = (ulong)Parameters[1]!;
        var amount = (int)Parameters[2]!;

        await InitAsync(guildId, userId);
        if (Guild is null || User is null)
            return ApiResult.Ok();

        await _pointsManager.PushSynchronizationAsync(Guild, User);

        var payload = new CreateTransactionAdminPayload(guildId.ToString(), userId.ToString(), amount);
        await _pointsManager.PushPayloadAsync(payload);

        return ApiResult.Ok();
    }

    private async Task InitAsync(ulong guildId, ulong userId)
    {
        Guild = await DiscordClient.GetGuildAsync(guildId);
        User = Guild is null ? null : await Guild.GetUserAsync(userId);

        if (User is null)
            throw new NotFoundException(Texts["Points/Service/Increment/UserNotFound", ApiContext.Language]);
        if (!await _pointsManager.IsUserAcceptableAsync(User))
            throw new ValidationException(Texts["Points/Service/Increment/NotAcceptable", ApiContext.Language]).ToBadRequestValidation(userId, nameof(userId));
    }
}
