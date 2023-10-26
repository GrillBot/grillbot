using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Actions.Api.V2.AuditLog;

public class CreateAuditLogMessageAction : ApiAction
{
    private IAuditLogServiceClient Client { get; }

    public CreateAuditLogMessageAction(ApiRequestContext apiContext, IAuditLogServiceClient client) : base(apiContext)
    {
        Client = client;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var request = (Data.Models.API.AuditLog.Public.LogMessageRequest)Parameters[0]!;
        var logRequest = new LogRequest
        {
            Type = request.Type.ToLower() switch
            {
                "warning" => LogType.Warning,
                "info" => LogType.Info,
                "error" => LogType.Error,
                _ => throw new NotSupportedException()
            },
            ChannelId = request.ChannelId,
            CreatedAt = DateTime.UtcNow,
            GuildId = request.GuildId,
            LogMessage = new LogMessageRequest
            {
                Message = request.Message,
                Severity = request.Type.ToLower() switch
                {
                    "warning" => LogSeverity.Warning,
                    "info" => LogSeverity.Info,
                    "error" => LogSeverity.Error,
                    _ => throw new NotSupportedException()
                },
                Source = request.MessageSource,
                SourceAppName = ApiContext.GetUsername()!
            },
            UserId = request.UserId
        };

        await Client.CreateItemsAsync(new List<LogRequest> { logRequest });
        return ApiResult.Ok();
    }
}
