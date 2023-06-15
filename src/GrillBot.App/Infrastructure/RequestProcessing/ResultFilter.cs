using GrillBot.Common.Models;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;
using GrillBot.Data.Models.AuditLog;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrillBot.App.Infrastructure.RequestProcessing;

public class ResultFilter : IAsyncResultFilter
{
    private ApiRequest ApiRequest { get; }
    private ApiRequestContext ApiRequestContext { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public ResultFilter(ApiRequest apiRequest, ApiRequestContext apiRequestContext, IAuditLogServiceClient auditLogServiceClient)
    {
        ApiRequest = apiRequest;
        AuditLogServiceClient = auditLogServiceClient;
        ApiRequestContext = apiRequestContext;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        var response = context.HttpContext.Response;
        ApiRequest.StatusCode = $"{response.StatusCode} ({(HttpStatusCode)response.StatusCode})";
        ApiRequest.EndAt = DateTime.UtcNow;

        await WriteToAuditLogAsync();
    }

    private async Task WriteToAuditLogAsync()
    {
        var userId = ApiRequestContext.GetUserId();
        var logRequest = new LogRequest
        {
            ApiRequest = new ApiRequestRequest
            {
                EndAt = ApiRequest.EndAt,
                Headers = ApiRequest.Headers,
                Identification = ApiRequest.UserIdentification,
                Ip = ApiRequest.IpAddress,
                Language = ApiRequest.Language,
                Method = ApiRequest.Method,
                Parameters = ApiRequest.Parameters,
                Path = ApiRequest.Path,
                ActionName = ApiRequest.ActionName,
                ControllerName = ApiRequest.ControllerName,
                StartAt = ApiRequest.StartAt,
                TemplatePath = ApiRequest.TemplatePath,
                ApiGroupName = ApiRequest.ApiGroupName!,
                Result = ApiRequest.StatusCode
            },
            Type = LogType.Api,
            CreatedAt = DateTime.UtcNow,
            UserId = userId > 0 ? userId.ToString() : null
        };

        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
    }
}
