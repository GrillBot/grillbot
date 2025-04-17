﻿using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Data.Models.API.Statistics;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetApiUserStatistics(
    ApiRequestContext apiContext,
    IServiceClientExecutor<IAuditLogServiceClient> _auditLogServiceClient,
    DataResolveManager _dataResolveManager
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var criteria = (string)Parameters[0]!;
        var statistics = await _auditLogServiceClient.ExecuteRequestAsync((c, ctx) => c.GetUserApiStatisticsAsync(criteria, ctx.CancellationToken));
        var result = new List<UserActionCountItem>();

        foreach (var item in statistics)
        {
            result.Add(new UserActionCountItem
            {
                Username = await ResolveUsernameAsync(item.UserId),
                Action = item.Action,
                Count = item.Count
            });
        }

        result = [.. result.OrderBy(o => o.Username).ThenBy(o => o.Action)];
        return ApiResult.Ok(result);
    }

    private async Task<string> ResolveUsernameAsync(string userId)
    {
        if (!ulong.TryParse(userId, CultureInfo.InvariantCulture, out var id))
            return userId;

        var user = await _dataResolveManager.GetUserAsync(id);
        return string.IsNullOrEmpty(user?.Username) ? userId : user.Username;
    }
}
