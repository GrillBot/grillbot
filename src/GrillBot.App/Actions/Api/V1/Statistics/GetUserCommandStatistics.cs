using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Data.Models.API.Statistics;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetUserCommandStatistics : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public GetUserCommandStatistics(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        AuditLogServiceClient = auditLogServiceClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var statistics = await AuditLogServiceClient.GetUserCommandStatisticsAsync();
        var userIds = statistics.Select(o => o.UserId).Distinct().ToList();

        await using var repository = DatabaseBuilder.CreateRepository();
        var users = await repository.User.GetUsersByIdsAsync(userIds);
        var usernames = users.ToDictionary(o => o.Id, o => o.Username);

        var result = statistics.Select(o => new UserActionCountItem
        {
            Username = usernames.TryGetValue(o.UserId, out var username) ? username : o.UserId,
            Action = o.Action,
            Count = o.Count
        }).OrderBy(o => o.Username).ThenBy(o => o.Action).ToList();

        return ApiResult.Ok(result);
    }
}
