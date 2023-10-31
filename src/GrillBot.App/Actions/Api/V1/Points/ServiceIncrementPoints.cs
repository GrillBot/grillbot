﻿using GrillBot.App.Helpers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Models;
using GrillBot.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V1.Points;

public class ServiceIncrementPoints : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private PointsHelper PointsHelper { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    private IGuild? Guild { get; set; }
    private IUser? User { get; set; }

    public ServiceIncrementPoints(ApiRequestContext apiContext, IDiscordClient discordClient, ITextsManager texts, PointsHelper pointsHelper, IPointsServiceClient pointsServiceClient) :
        base(apiContext)
    {
        DiscordClient = discordClient;
        Texts = texts;
        PointsHelper = pointsHelper;
        PointsServiceClient = pointsServiceClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (ulong)Parameters[0]!;
        var userId = (ulong)Parameters[1]!;
        var amount = (int)Parameters[2]!;

        await InitAsync(guildId, userId);
        if (Guild is null || User is null)
            return ApiResult.Ok();

        var request = new AdminTransactionRequest
        {
            GuildId = guildId.ToString(),
            Amount = amount,
            UserId = userId.ToString()
        };

        var validationErrors = await PointsServiceClient.CreateTransactionAsync(request);
        if (PointsHelper.CanSyncData(validationErrors))
        {
            await PointsHelper.SyncDataWithServiceAsync(Guild, new[] { User }, Enumerable.Empty<IGuildChannel>());
            validationErrors = await PointsServiceClient.CreateTransactionAsync(request);
        }

        var exception = ConvertValidationErrorsToException(validationErrors);
        if (exception is not null)
            throw exception;
        return ApiResult.Ok();
    }

    private async Task InitAsync(ulong guildId, ulong userId)
    {
        Guild = await DiscordClient.GetGuildAsync(guildId);
        User = Guild is null ? null : await Guild.GetUserAsync(userId);

        if (User is null)
            throw new NotFoundException(Texts["Points/Service/Increment/UserNotFound", ApiContext.Language]);
    }

    private Exception? ConvertValidationErrorsToException(ValidationProblemDetails? details)
    {
        if (details is null)
            return null;

        if (details.Errors.Any(o => o.Key == "Request" && o.Value.Contains("NotAcceptable")))
            return new ValidationException(Texts["Points/Service/Increment/NotAcceptable", ApiContext.Language]);

        var error = details.Errors.First();
        return new ValidationException(error.Value[0]).ToBadRequestValidation(null, error.Key);
    }
}
