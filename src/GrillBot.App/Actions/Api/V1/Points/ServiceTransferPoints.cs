using GrillBot.App.Helpers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Models;
using GrillBot.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

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

    public async Task ProcessAsync(ulong guildId, ulong fromUserId, ulong toUserId, int amount)
    {
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
        var user = await Guild.GetUserAsync(userId);

        if (user is null)
            throw new NotFoundException(Texts[$"Points/Service/Transfer/{(isSource ? "SourceUserNotFound" : "DestUserNotFound")}", ApiContext.Language]);
        return user;
    }

    private Exception? ConvertValidationErrorsToException(ValidationProblemDetails? details)
    {
        if (details is null)
            return null;

        var error = details.Errors.First();

        if (error.Key.ToLower() == "Amount" && error.Value.First() == "NotEnoughPoints")
            return new ValidationException(Texts["Points/Service/Transfer/InsufficientAmount", ApiContext.Language]);
        if (error.Value.First() == "User is bot.")
            new ValidationException(Texts["Points/Service/Transfer/UserIsBot", ApiContext.Language]).ToBadRequestValidation(null, error.Key);

        return new ValidationException(error.Value.First()).ToBadRequestValidation(null, error.Key);
    }
}
