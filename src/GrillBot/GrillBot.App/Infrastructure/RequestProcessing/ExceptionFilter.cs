using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.Common.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrillBot.App.Infrastructure.RequestProcessing;

public class ExceptionFilter : IAsyncExceptionFilter
{
    private LoggingService LoggingService { get; }
    private ApiRequest ApiRequest { get; }
    private AuditLogWriter AuditLogWriter { get; }
    private ApiRequestContext ApiRequestContext { get; }

    public ExceptionFilter(LoggingService loggingService, ApiRequest apiRequest, AuditLogWriter auditLogWriter, ApiRequestContext apiRequestContext)
    {
        LoggingService = loggingService;
        ApiRequest = apiRequest;
        AuditLogWriter = auditLogWriter;
        ApiRequestContext = apiRequestContext;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        ApiRequest.EndAt = DateTime.Now;

        if (context.Exception is OperationCanceledException)
        {
            context.ExceptionHandled = true;
            context.Result = new StatusCodeResult(400);
            ApiRequest.StatusCode = "400 Bad Request";
            return;
        }

        ApiRequest.StatusCode = "500 Internal Server Error";

        var wrapper = new AuditLogDataWrapper(AuditLogItemType.Api, ApiRequest, null, null, ApiRequestContext.LoggedUser);
        await AuditLogWriter.StoreAsync(wrapper);

        var path = context.HttpContext.Request.Path;
        await LoggingService.ErrorAsync("API", $"An error occured while request processing ({path})", context.Exception);
    }
}
