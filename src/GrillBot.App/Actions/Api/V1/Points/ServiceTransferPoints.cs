using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.App.Managers.Points;
using GrillBot.Core.Services.Common;

namespace GrillBot.App.Actions.Api.V1.Points;

public class ServiceTransferPoints : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    private IGuild Guild { get; set; } = null!;

    private readonly PointsManager _pointsManager;

    public ServiceTransferPoints(ApiRequestContext apiContext, IDiscordClient discordClient, ITextsManager texts, IPointsServiceClient pointsServiceClient, PointsManager pointsManager) :
        base(apiContext)
    {
        DiscordClient = discordClient;
        Texts = texts;
        PointsServiceClient = pointsServiceClient;
        _pointsManager = pointsManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (ulong)Parameters[0]!;
        var fromUserId = (ulong)Parameters[1]!;
        var toUserId = (ulong)Parameters[2]!;
        var amount = (int)Parameters[3]!;

        var (from, to) = await GetAndCheckUsersAsync(guildId, fromUserId, toUserId);
        await _pointsManager.PushSynchronizationAsync(Guild, from, to);

        var request = new TransferPointsRequest
        {
            GuildId = guildId.ToString(),
            Amount = amount,
            FromUserId = fromUserId.ToString(),
            ToUserId = toUserId.ToString()
        };

        while (true)
        {
            try
            {
                await PointsServiceClient.TransferPointsAsync(request);
            }
            catch (ClientBadRequestException ex)
            {
                if (PointsValidationManager.IsMissingData(ex.ValidationErrors))
                {
                    await Task.Delay(1000);
                    continue;
                }

                var exception = ConvertValidationErrorsToException(ex.ValidationErrors);
                if (exception is not null)
                    throw exception;
            }

            break;
        }

        return ApiResult.Ok();
    }

    private async Task<(IGuildUser from, IGuildUser to)> GetAndCheckUsersAsync(ulong guildId, ulong fromUserId, ulong toUserId)
    {
        if (fromUserId == toUserId)
        {
            throw new ValidationException(Texts["Points/Service/Transfer/SameAccounts", ApiContext.Language])
                .ToBadRequestValidation($"{fromUserId}->{toUserId}", nameof(fromUserId), nameof(toUserId));
        }

        Guild = await DiscordClient.GetGuildAsync(guildId);
        if (Guild is null)
            throw new NotFoundException(Texts["Points/Service/Transfer/GuildNotFound", ApiContext.Language]);

        return (
            await CheckUserAsync(fromUserId, true),
            await CheckUserAsync(toUserId, false)
        );
    }

    private async Task<IGuildUser> CheckUserAsync(ulong userId, bool isSource)
    {
        return await Guild.GetUserAsync(userId)
            ?? throw new NotFoundException(Texts[$"Points/Service/Transfer/{(isSource ? "SourceUserNotFound" : "DestUserNotFound")}", ApiContext.Language]);
    }

    private Exception? ConvertValidationErrorsToException(Dictionary<string, string[]> validationErrors)
    {
        var error = validationErrors.First();

        if (string.Equals(error.Key, "amount", StringComparison.OrdinalIgnoreCase) && error.Value[0] == "NotEnoughPoints")
            return new ValidationException(Texts["Points/Service/Transfer/InsufficientAmount", ApiContext.Language]);
        if (error.Value[0] == "User is bot.")
            new ValidationException(Texts["Points/Service/Transfer/UserIsBot", ApiContext.Language]).ToBadRequestValidation(null, error.Key);

        return new ValidationException(error.Value[0]).ToBadRequestValidation(null, error.Key);
    }
}
