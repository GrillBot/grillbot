using GrillBot.App.Services.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrillBot.App.Infrastructure.RequestProcessing;

public class ExceptionFilter : IAsyncExceptionFilter
{
    private LoggingService LoggingService { get; }

    public ExceptionFilter(LoggingService loggingService)
    {
        LoggingService = loggingService;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is OperationCanceledException)
        {
            context.ExceptionHandled = true;
            context.Result = new StatusCodeResult(400);
            return;
        }

        var path = context.HttpContext.Request.Path;
        await LoggingService.ErrorAsync("API", $"An error occured while request processing ({path})", context.Exception);
    }
}
