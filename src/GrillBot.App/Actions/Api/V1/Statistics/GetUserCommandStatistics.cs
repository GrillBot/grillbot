using GrillBot.App.Managers;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetUserCommandStatistics : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetUserCommandStatistics(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<List<UserActionCountItem>> ProcessAsync()
    {
        var logs = await GetLogsAsync();

        return logs
            .Select(o => new { Item = o, Data = JsonConvert.DeserializeObject<InteractionCommandExecuted>(o.Data, AuditLogWriteManager.SerializerSettings)! })
            .GroupBy(o => new { Username = o.Item.ProcessedUser!.FullName(), o.Data.FullName })
            .Select(o => new UserActionCountItem(o.Key.Username, o.Key.FullName, o.Count()))
            .OrderBy(o => o.Username).ThenBy(o => o.Action)
            .ToList();
    }

    private async Task<List<AuditLogItem>> GetLogsAsync()
    {
        var parameters = new AuditLogListParams
        {
            Sort = null,
            Types = new List<AuditLogItemType> { AuditLogItemType.InteractionCommand },
            Pagination = { Page = 0, PageSize = int.MaxValue }
        };

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.AuditLog.GetLogListAsync(parameters, parameters.Pagination, null);
        return data.Data;
    }
}
