﻿using GrillBot.Common.Models;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrillBot.App.Infrastructure.RequestProcessing;

public class ResultFilter : IAsyncResultFilter
{
    private ApiRequestContext ApiRequestContext { get; }

    private readonly IRabbitMQPublisher _rabbitPublisher;

    public ResultFilter(ApiRequestContext apiRequestContext, IRabbitMQPublisher rabbitPublisher)
    {
        ApiRequestContext = apiRequestContext;
        _rabbitPublisher = rabbitPublisher;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        var response = context.HttpContext.Response;
        ApiRequestContext.LogRequest.Result = $"{response.StatusCode} ({(HttpStatusCode)response.StatusCode})";
        ApiRequestContext.LogRequest.EndAt = DateTime.UtcNow;

        await WriteToAuditLogAsync();
    }

    private Task WriteToAuditLogAsync()
    {
        var userId = ApiRequestContext.GetUserId().ToString();
        if (userId == "0") userId = null;

        var logRequest = new LogRequest(LogType.Api, DateTime.UtcNow, null, userId)
        {
            ApiRequest = ApiRequestContext.LogRequest,
        };

        return _rabbitPublisher.PublishAsync(new CreateItemsPayload(logRequest), new());
    }
}
