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
        await ProcessAsync((CreateUserMeasuresWarningParams)Parameters[0]!);
        return ApiResult.Ok();
    }

    public async Task ProcessAsync(IGuildUser user, string message)
    {
        var parameters = new CreateUserMeasuresWarningParams
        {
            GuildId = user.GuildId.ToString(),
            Message = message,
            UserId = user.Id.ToString()
        };

        await ProcessAsync(parameters);
    }

    private async Task ProcessAsync(CreateUserMeasuresWarningParams parameters)
    {
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
    }
}
