using GrillBot.App.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.GrillBot.Models;
using UnverifyService;
using UnverifyService.Models.Events;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class RecoverState(
    ApiRequestContext apiContext,
    ITextsManager _texts,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient,
    IRabbitPublisher _rabbitPublisher
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var logId = (long)Parameters[0]!;

        try
        {
            await _unverifyClient.ExecuteRequestAsync(
                async (client, ctx) => await client.CheckRecoveryRequirementsAsync(logNumber: logId, cancellationToken: CancellationToken),
                CancellationToken
            );
        }
        catch (ClientBadRequestException ex)
        {
            if (string.IsNullOrEmpty(ex.RawData))
                throw;

            var validationError = JsonConvert.DeserializeObject<LocalizedMessageContent>(ex.RawData)!;
            var errorMessage = _texts[validationError, ApiContext.Language];

            throw new ValidationException(errorMessage).ToBadRequestValidation(logId, nameof(logId));
        }

        var payload = new RecoverAccessMessage
        {
            LogNumber = logId
        };

        await _rabbitPublisher.PublishAsync(payload, cancellationToken: CancellationToken);
        return ApiResult.Ok();
    }
}
