using GrillBot.Common.Exceptions;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;
using GrillBot.Core.Exceptions;
using GrillBot.Data.Models.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GrillBot.App.Infrastructure.RequestProcessing;

public class ExceptionFilter : IAsyncExceptionFilter
{
    private ApiRequestContext ApiRequestContext { get; }
    private LoggingManager LoggingManager { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public ExceptionFilter(ApiRequestContext apiRequestContext, LoggingManager loggingManager, IAuditLogServiceClient auditLogServiceClient)
    {
        ApiRequestContext = apiRequestContext;
        LoggingManager = loggingManager;
        AuditLogServiceClient = auditLogServiceClient;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        ApiRequestContext.LogRequest.EndAt = DateTime.UtcNow;

        switch (context.Exception)
        {
            case OperationCanceledException:
                context.ExceptionHandled = true;
                context.Result = new StatusCodeResult(400);
                ApiRequestContext.LogRequest.Result = "400 (BadRequest)";
                break;
            case ValidationException validationException:
                if (validationException.ValidationResult.MemberNames.Any())
                    SetValidationError(context, validationException);
                break;
            case NotFoundException:
                SetNotFound(context);
                break;
            case ForbiddenAccessException:
                SetForbidden(context);
                break;
            case GrillBotException:
                context.Result = new ObjectResult(new MessageResponse(context.Exception.Message)) { StatusCode = StatusCodes.Status500InternalServerError };
                break;
            case AggregateException aggregateException:
                var validationExceptions = aggregateException.InnerExceptions.OfType<ValidationException>().Where(o => o.ValidationResult.MemberNames.Any()).ToList();
                if (validationExceptions.Count > 0)
                    SetValidationErrors(context, validationExceptions);
                break;
        }

        if (context.ExceptionHandled) return;
        if (string.IsNullOrEmpty(ApiRequestContext.LogRequest.Result))
            ApiRequestContext.LogRequest.Result = "500 (InternalServerError)";

        await WriteToAuditLogAsync();
        
        var path = $"{ApiRequestContext.LogRequest.Method} {ApiRequestContext.LogRequest.Path}";
        var controllerInfo = $"{ApiRequestContext.LogRequest.ControllerName}.{ApiRequestContext.LogRequest.ActionName}";
        var exception = new ApiException(context.Exception.Message, context.Exception, ApiRequestContext.LoggedUser, path, controllerInfo);
        await LoggingManager.ErrorAsync("API", "An error occured while request processing API request", exception);
    }

    private void SetValidationError(ExceptionContext context, ValidationException ex)
    {
        var validationResult = ex.ValidationResult;
        var modelState = new ModelStateDictionary();
        foreach (var memberName in validationResult.MemberNames)
            modelState.AddModelError(memberName, validationResult.ErrorMessage ?? "");

        context.ExceptionHandled = true;
        context.Result = new BadRequestObjectResult(new ValidationProblemDetails(modelState));
        ApiRequestContext.LogRequest.Result = "400 (BadRequest)";
    }

    private void SetValidationErrors(ExceptionContext context, IEnumerable<ValidationException> exceptions)
    {
        var modelState = new ModelStateDictionary();
        foreach (var exception in exceptions)
        {
            foreach (var memberName in exception.ValidationResult.MemberNames)
                modelState.AddModelError(memberName, exception.ValidationResult.ErrorMessage ?? "");
        }

        context.ExceptionHandled = true;
        context.Result = new BadRequestObjectResult(new ValidationProblemDetails(modelState));
        ApiRequestContext.LogRequest.Result = "400 (BadRequest)";
    }

    private void SetNotFound(ExceptionContext context)
    {
        context.ExceptionHandled = true;
        context.Result = new NotFoundObjectResult(new MessageResponse(context.Exception.Message));
        ApiRequestContext.LogRequest.Result = "404 (NotFound)";
    }

    private void SetForbidden(ExceptionContext context)
    {
        context.ExceptionHandled = true;
        context.Result = new ObjectResult(new MessageResponse(context.Exception.Message)) { StatusCode = StatusCodes.Status403Forbidden };
        ApiRequestContext.LogRequest.Result = "403 (Forbidden)";
    }

    private async Task WriteToAuditLogAsync()
    {
        var userId = ApiRequestContext.GetUserId();
        var logRequest = new LogRequest
        {
            Type = LogType.Api,
            UserId = userId > 0 ? userId.ToString() : null,
            CreatedAt = DateTime.UtcNow,
            ApiRequest = ApiRequestContext.LogRequest
        };

        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
    }
}
