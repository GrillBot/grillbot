using GrillBot.App.Managers;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetApiUserStatistics : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    
    public GetApiUserStatistics(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }
    
    public async Task<List<UserActionCountItem>> ProcessAsync()
    {
        var logs = await GetLogsAsync();

        return logs
            .Select(o => new { Item = o, Data = JsonConvert.DeserializeObject<ApiRequest>(o.Data, AuditLogWriteManager.SerializerSettings)! })
            .Where(o => !o.Data.IsCorrupted() && (o.Item.ProcessedUser != null || o.Data.UserIdentification != null))
            .GroupBy(o => new { Username = o.Item.ProcessedUser?.FullName() ?? o.Data.UserIdentification, o.Data.TemplatePath })
            .Where(o => !string.IsNullOrEmpty(o.Key.Username))
            .Select(o => new UserActionCountItem(o.Key.Username!, o.Key.TemplatePath, o.Count()))
            .OrderBy(o => o.Username).ThenBy(o => o.Action)
            .ToList();
    }

    private async Task<List<AuditLogItem>> GetLogsAsync()
    {
        var parameters = new AuditLogListParams
        {
            Sort = null,
            Types = new List<AuditLogItemType> { AuditLogItemType.Api },
            Pagination = { Page = 0, PageSize = int.MaxValue }
        };

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.AuditLog.GetLogListAsync(parameters, parameters.Pagination, null);
        return data.Data;
    }
}
