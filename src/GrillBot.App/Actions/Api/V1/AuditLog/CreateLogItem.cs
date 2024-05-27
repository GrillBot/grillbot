using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using GrillBot.Data.Models.API.AuditLog;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class CreateLogItem : ApiAction
{
    private ITextsManager Texts { get; }
    private IRabbitMQPublisher RabbitPublisher { get; }

    public CreateLogItem(ApiRequestContext apiContext, ITextsManager texts, IRabbitMQPublisher rabbitPublisher) : base(apiContext)
    {
        Texts = texts;
        RabbitPublisher = rabbitPublisher;
    }

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
        {
            logRequest.Type = LogType.Info;
            logRequest.LogMessage.Severity = LogSeverity.Info;
        }
        else if (request.IsError)
        {
            logRequest.Type = LogType.Error;
            logRequest.LogMessage.Severity = LogSeverity.Error;
        }
        else if (request.IsWarning)
        {
            logRequest.Type = LogType.Warning;
            logRequest.LogMessage.Severity = LogSeverity.Warning;
        }

        await RabbitPublisher.PublishAsync(new CreateItemsPayload(logRequest), new());
        return ApiResult.Ok();
    }

    private void ValidateParameters(ClientLogItemRequest request)
    {
        var flags = new[] { request.IsInfo, request.IsWarning, request.IsError };
        var names = new[] { nameof(request.IsInfo), nameof(request.IsWarning), nameof(request.IsError) };

        string? errorMessage = null;
        if (!Array.Exists(flags, o => o))
            errorMessage = Texts["AuditLog/CreateLogItem/Required", ApiContext.Language];
        else if (flags.Count(o => o) > 1)
            errorMessage = Texts["AuditLog/CreateLogItem/MultipleTypes", ApiContext.Language];

        if (!string.IsNullOrEmpty(errorMessage))
            throw new ValidationException(errorMessage).ToBadRequestValidation(request, names);
    }
}
