using AuditLogService.Models.Request;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;
using GrillBot.Data.Models.API.AuditLog;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class CreateLogItem : ApiAction
{
    private ITextsManager Texts { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public CreateLogItem(ApiRequestContext apiContext, ITextsManager texts, IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        Texts = texts;
        AuditLogServiceClient = auditLogServiceClient;
    }

    public async Task ProcessAsync(ClientLogItemRequest request)
    {
        ValidateParameters(request);

        var logRequest = new LogRequest
        {
            UserId = ApiContext.GetUserId().ToString(),
            LogMessage = new LogMessageRequest
            {
                Message = request.Content
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

        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
    }

    private void ValidateParameters(ClientLogItemRequest request)
    {
        var flags = new[] { request.IsInfo, request.IsWarning, request.IsError };
        var names = new[] { nameof(request.IsInfo), nameof(request.IsWarning), nameof(request.IsError) };

        ValidationResult? result = null;
        if (!flags.Any(o => o))
            result = new ValidationResult(Texts["AuditLog/CreateLogItem/Required", ApiContext.Language], names);
        else if (flags.Count(o => o) > 1)
            result = new ValidationResult(Texts["AuditLog/CreateLogItem/MultipleTypes", ApiContext.Language], names);

        if (result is not null)
            throw new ValidationException(result, null, request);
    }
}
