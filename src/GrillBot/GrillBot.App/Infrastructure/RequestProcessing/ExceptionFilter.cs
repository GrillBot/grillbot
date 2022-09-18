using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Exceptions;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

        switch (context.Exception)
        {
            case OperationCanceledException:
                context.ExceptionHandled = true;
                context.Result = new StatusCodeResult(400);
                ApiRequest.StatusCode = "400 (BadRequest)";
                return;
            case ValidationException validationException:
                if (validationException.ValidationResult.MemberNames.Any())
                {
                    SetValidationError(context, validationException);
                    return;
                }

                break;
        }

        ApiRequest.StatusCode = "500 (InternalServerError)";
        if (context.Exception is GrillBotException)
            context.Result = new ObjectResult(new MessageResponse(context.Exception.Message)) { StatusCode = StatusCodes.Status500InternalServerError };

        var wrapper = new AuditLogDataWrapper(AuditLogItemType.Api, ApiRequest, null, null, ApiRequestContext.LoggedUser);
        await AuditLogWriter.StoreAsync(wrapper);

        var path = $"{ApiRequest.Method} {ApiRequest.Path}";
        var controllerInfo = $"{ApiRequest.ControllerName}.{ApiRequest.ActionName}";
        var exception = new ApiException(context.Exception.Message, context.Exception, ApiRequestContext.LoggedUser, path, controllerInfo);
        await LoggingManager.ErrorAsync("API", "An error occured while request processing API request", exception);
    }

    private void SetValidationError(ExceptionContext context, ValidationException ex)
    {
        var validationResult = ex.ValidationResult;
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage ?? "");

        context.ExceptionHandled = true;
        context.Result = new BadRequestObjectResult(new ValidationProblemDetails(modelState));
        ApiRequest.StatusCode = "400 (BadRequest)";
    }
}
