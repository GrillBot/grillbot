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

    public async Task<List<UserActionCountItem>> ProcessAsync(string criteria)
    {
        var logs = await GetLogsAsync();

        return logs
            .Select(ReadItem)
            .Where(o => CanProcess(o.item, o.request, criteria))
            .GroupBy(o => new { Username = o.item.ProcessedUser?.FullName() ?? o.request.UserIdentification, o.request.TemplatePath })
            .Where(o => !string.IsNullOrEmpty(o.Key.Username))
            .Select(o => new UserActionCountItem(o.Key.Username!, o.Key.TemplatePath, o.Count()))
            .OrderBy(o => o.Username).ThenBy(o => o.Action)
            .ToList();
    }

    private static (AuditLogItem item, ApiRequest request) ReadItem(AuditLogItem item)
        => (item, JsonConvert.DeserializeObject<ApiRequest>(item.Data, AuditLogWriteManager.SerializerSettings)!);

    private static bool CanProcess(AuditLogItem item, ApiRequest request, string criteria)
    {
        if (request.IsCorrupted())
            return false;

        if (item.ProcessedUser is null && string.IsNullOrEmpty(request.UserIdentification))
            return false;

        return criteria switch
        {
            "v1-private" => (request.ApiGroupName ?? "V1").ToUpper() == "V1" && request.LoggedUserRole.ToLower() == "admin",
            "v1-public" => (request.ApiGroupName ?? "V1").ToUpper() == "V1" && request.LoggedUserRole.ToLower() == "user",
            "v2" => (request.ApiGroupName ?? "V1").ToUpper() == "V2",
            _ => throw new NotSupportedException("Unsupported criteria.")
        };
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
