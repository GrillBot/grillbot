using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using AuditLog.Enums;
using AuditLog.Models.Events.Create;
using GrillBot.Data.Models.API.AuditLog;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class CreateLogItem(
    ApiRequestContext apiContext,
    ITextsManager _texts,
    IRabbitPublisher _rabbitPublisher
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var request = (ClientLogItemRequest)Parameters[0]!;
        ValidateParameters(request);

        var logRequest = new LogRequest
        {
            CreatedAtUtc = DateTime.UtcNow,
            UserId = ApiContext.GetUserId().ToString(),
            LogMessage = new LogMessageRequest
            {
                Message = request.Content,
                Source = request.Source,
                SourceAppName = request.AppName
            }
        };

        if (request.IsInfo)
            logRequest.Type = LogType.Info;
        else if (request.IsError)
            logRequest.Type = LogType.Error;
        else if (request.IsWarning)
            logRequest.Type = LogType.Warning;

        await _rabbitPublisher.PublishAsync(new CreateItemsMessage(logRequest));
        return ApiResult.Ok();
    }

    private void ValidateParameters(ClientLogItemRequest request)
    {
        var flags = new[] { request.IsInfo, request.IsWarning, request.IsError };
        var names = new[] { nameof(request.IsInfo), nameof(request.IsWarning), nameof(request.IsError) };

        string? errorMessage = null;
        if (!Array.Exists(flags, o => o))
            errorMessage = _texts["AuditLog/CreateLogItem/Required", ApiContext.Language];
        else if (flags.Count(o => o) > 1)
            errorMessage = _texts["AuditLog/CreateLogItem/MultipleTypes", ApiContext.Language];

        if (!string.IsNullOrEmpty(errorMessage))
            throw new ValidationException(errorMessage).ToBadRequestValidation(request, names);
    }
}
