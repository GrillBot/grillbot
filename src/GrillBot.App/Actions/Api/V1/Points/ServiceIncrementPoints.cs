using GrillBot.App.Helpers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.PointsService.Models.Events;

namespace GrillBot.App.Actions.Api.V1.Points;

public class ServiceIncrementPoints : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private PointsHelper PointsHelper { get; }
    private IRabbitMQPublisher RabbitMQ { get; }

    private IGuild? Guild { get; set; }
    private IUser? User { get; set; }

    public ServiceIncrementPoints(ApiRequestContext apiContext, IDiscordClient discordClient, ITextsManager texts, PointsHelper pointsHelper, IRabbitMQPublisher rabbitMQ) :
        base(apiContext)
    {
        DiscordClient = discordClient;
        Texts = texts;
        PointsHelper = pointsHelper;
        RabbitMQ = rabbitMQ;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (ulong)Parameters[0]!;
        var userId = (ulong)Parameters[1]!;
        var amount = (int)Parameters[2]!;

        await InitAsync(guildId, userId);
        if (Guild is null || User is null)
            return ApiResult.Ok();

        await PointsHelper.PushSynchronizationAsync(Guild, User);

        var payload = new CreateTransactionAdminPayload
        {
            Amount = amount,
            GuildId = guildId.ToString(),
            UserId = userId.ToString()
        };

        await RabbitMQ.PublishAsync(CreateTransactionAdminPayload.QueueName, payload);
        return ApiResult.Ok();
    }

    private async Task InitAsync(ulong guildId, ulong userId)
    {
        Guild = await DiscordClient.GetGuildAsync(guildId);
        User = Guild is null ? null : await Guild.GetUserAsync(userId);

        if (User is null)
            throw new NotFoundException(Texts["Points/Service/Increment/UserNotFound", ApiContext.Language]);
        if (!await PointsHelper.IsUserAcceptableAsync(User))
            throw new ValidationException(Texts["Points/Service/Increment/NotAcceptable", ApiContext.Language]).ToBadRequestValidation(userId, nameof(userId));
    }
}
