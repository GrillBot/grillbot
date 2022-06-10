using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrillBot.App.Infrastructure.RequestProcessing;

public class ResultFilter : IAsyncResultFilter
{
    private ApiRequest ApiRequest { get; }
    private AuditLogService AuditLogService { get; }
    private IDiscordClient DiscordClient { get; }

    public ResultFilter(ApiRequest apiRequest, AuditLogService auditLogService, IDiscordClient discordClient)
    {
        ApiRequest = apiRequest;
        AuditLogService = auditLogService;
        DiscordClient = discordClient;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        ApiRequest.EndAt = DateTime.Now;

        var response = context.HttpContext.Response;
        ApiRequest.StatusCode = $"{response.StatusCode} ({(HttpStatusCode)response.StatusCode})";

        var userId = context.HttpContext.User.GetUserId();
        var processedUser = userId > 0 ? await DiscordClient.FindUserAsync(userId) : null;

        var wrapper = new AuditLogDataWrapper(AuditLogItemType.Api, ApiRequest, null, null, processedUser, null, DateTime.Now, null);
        await AuditLogService.StoreItemAsync(wrapper);
    }
}
