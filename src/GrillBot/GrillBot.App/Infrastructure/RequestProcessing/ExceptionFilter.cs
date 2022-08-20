using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrillBot.App.Infrastructure.RequestProcessing;

public class ExceptionFilter : IAsyncExceptionFilter
{
    private ApiRequest ApiRequest { get; }
    private AuditLogWriter AuditLogWriter { get; }
    private ApiRequestContext ApiRequestContext { get; }
    private LoggingManager LoggingManager { get; }

    public ExceptionFilter(ApiRequest apiRequest, AuditLogWriter auditLogWriter, ApiRequestContext apiRequestContext, LoggingManager loggingManager)
    {
        ApiRequest = apiRequest;
        AuditLogWriter = auditLogWriter;
        ApiRequestContext = apiRequestContext;
        LoggingManager = loggingManager;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        ApiRequest.EndAt = DateTime.Now;

        if (context.Exception is OperationCanceledException)
        {
            context.ExceptionHandled = true;
            context.Result = new StatusCodeResult(400);
            ApiRequest.StatusCode = "400 (BadRequest)";
            return;
        }

        ApiRequest.StatusCode = "500 (InternalServerError)";

        var wrapper = new AuditLogDataWrapper(AuditLogItemType.Api, ApiRequest, null, null, ApiRequestContext.LoggedUser);
        await AuditLogWriter.StoreAsync(wrapper);

        var path = context.HttpContext.Request.Path;
        await LoggingManager.ErrorAsync("API", $"An error occured while request processing ({path})", context.Exception);
    }
}
