using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;
using GrillBot.Data.Models.API.UserMeasures;

namespace GrillBot.App.Actions.Api.V1.UserMeasures;

public class CreateUserMeasuresWarning : ApiAction
{
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public CreateUserMeasuresWarning(ApiRequestContext apiContext, IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        AuditLogServiceClient = auditLogServiceClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (CreateUserMeasuresWarningParams)Parameters[0]!;

        var logRequest = new LogRequest
        {
            CreatedAt = DateTime.UtcNow,
            GuildId = parameters.GuildId,
            MemberWarning = new MemberWarningRequest
            {
                Reason = parameters.Message,
                TargetId = parameters.UserId
            },
            Type = LogType.MemberWarning,
            UserId = ApiContext.GetUserId().ToString()
        };

        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
        return ApiResult.Ok();
    }
}
