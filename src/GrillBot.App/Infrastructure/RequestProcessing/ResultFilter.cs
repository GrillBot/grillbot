using GrillBot.Common.Models;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrillBot.App.Infrastructure.RequestProcessing;

public class ResultFilter : IAsyncResultFilter
{
    private ApiRequestContext ApiRequestContext { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public ResultFilter(ApiRequestContext apiRequestContext, IAuditLogServiceClient auditLogServiceClient)
    {
        AuditLogServiceClient = auditLogServiceClient;
        ApiRequestContext = apiRequestContext;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        var response = context.HttpContext.Response;
        ApiRequestContext.LogRequest.Result = $"{response.StatusCode} ({(HttpStatusCode)response.StatusCode})";
        ApiRequestContext.LogRequest.EndAt = DateTime.UtcNow;

        await WriteToAuditLogAsync();
    }

    private async Task WriteToAuditLogAsync()
    {
        var userId = ApiRequestContext.GetUserId();
        var logRequest = new LogRequest
        {
            ApiRequest = ApiRequestContext.LogRequest,
            Type = LogType.Api,
            CreatedAt = DateTime.UtcNow,
            UserId = userId > 0 ? userId.ToString() : null
        };

        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
    }
}
