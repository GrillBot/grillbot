using GrillBot.App.Managers;
using GrillBot.Common.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrillBot.App.Infrastructure.RequestProcessing;

public class ResultFilter : IAsyncResultFilter
{
    private ApiRequest ApiRequest { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }
    private ApiRequestContext ApiRequestContext { get; }

    public ResultFilter(ApiRequest apiRequest, AuditLogWriteManager auditLogWriteManager, ApiRequestContext apiRequestContext)
    {
        ApiRequest = apiRequest;
        AuditLogWriteManager = auditLogWriteManager;
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
        await AuditLogWriteManager.StoreAsync(wrapper);
    }
}
