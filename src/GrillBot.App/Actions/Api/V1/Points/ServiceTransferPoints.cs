using GrillBot.App.Helpers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Models;
using GrillBot.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V1.Points;

public class ServiceTransferPoints : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private PointsHelper PointsHelper { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    private IGuild Guild { get; set; } = null!;

    public ServiceTransferPoints(ApiRequestContext apiContext, IDiscordClient discordClient, ITextsManager texts, PointsHelper pointsHelper, IPointsServiceClient pointsServiceClient) :
        base(apiContext)
    {
        DiscordClient = discordClient;
        PointsHelper = pointsHelper;
        Texts = texts;
        PointsServiceClient = pointsServiceClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (ulong)Parameters[0]!;
        var fromUserId = (ulong)Parameters[1]!;
        var toUserId = (ulong)Parameters[2]!;
        var amount = (int)Parameters[3]!;

        var (from, to) = await GetAndCheckUsersAsync(guildId, fromUserId, toUserId);

        var request = new TransferPointsRequest
        {
            GuildId = guildId.ToString(),
            Amount = amount,
            FromUserId = fromUserId.ToString(),
            ToUserId = toUserId.ToString()
        };

        var validationErrors = await PointsServiceClient.TransferPointsAsync(request);
        if (PointsHelper.CanSyncData(validationErrors))
        {
            await PointsHelper.SyncDataWithServiceAsync(Guild, new[] { from, to }, Enumerable.Empty<IGuildChannel>());
            validationErrors = await PointsServiceClient.TransferPointsAsync(request);
        }

        var exception = ConvertValidationErrorsToException(validationErrors);
        if (exception is not null)
            throw exception;
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
        if (Guild == null)
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

    private Exception? ConvertValidationErrorsToException(ValidationProblemDetails? details)
    {
        if (details is null)
            return null;

        var error = details.Errors.First();

        if (error.Key.ToLower() == "amount" && error.Value[0] == "NotEnoughPoints")
            return new ValidationException(Texts["Points/Service/Transfer/InsufficientAmount", ApiContext.Language]);
        if (error.Value[0] == "User is bot.")
            new ValidationException(Texts["Points/Service/Transfer/UserIsBot", ApiContext.Language]).ToBadRequestValidation(null, error.Key);

        return new ValidationException(error.Value[0]).ToBadRequestValidation(null, error.Key);
    }
}
