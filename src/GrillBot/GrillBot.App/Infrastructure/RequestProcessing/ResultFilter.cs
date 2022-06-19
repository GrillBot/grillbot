using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrillBot.App.Infrastructure.RequestProcessing;

public class ResultFilter : IAsyncResultFilter
{
    private ApiRequest ApiRequest { get; }
    private AuditLogWriter AuditLogWriter { get; }
    private ApiRequestContext ApiRequestContext { get; }

    public ResultFilter(ApiRequest apiRequest, AuditLogWriter auditLogWriter, ApiRequestContext apiRequestContext)
    {
        ApiRequest = apiRequest;
        AuditLogWriter = auditLogWriter;
        ApiRequestContext = apiRequestContext;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        var response = context.HttpContext.Response;
        ApiRequest.StatusCode = $"{response.StatusCode} ({(HttpStatusCode)response.StatusCode})";
        ApiRequest.EndAt = DateTime.Now;

        var processedUser = ApiRequestContext.LoggedUser;
        var wrapper = new AuditLogDataWrapper(AuditLogItemType.Api, ApiRequest, null, null, processedUser, null, DateTime.Now);
        await AuditLogWriter.StoreAsync(wrapper);
    }
}
