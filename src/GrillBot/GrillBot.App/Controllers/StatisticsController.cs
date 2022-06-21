using GrillBot.App.Services.AuditLog;
using GrillBot.Cache.Services;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/stats")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class StatisticsController : Controller
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private GrillBotCacheBuilder CacheBuilder { get; }

    public StatisticsController(GrillBotDatabaseBuilder databaseBuilder, GrillBotCacheBuilder cacheBuilder)
    {
        DatabaseBuilder = databaseBuilder;
        CacheBuilder = cacheBuilder;
    }

    /// <summary>
    /// Get statistics about database tables.
    /// </summary>
    /// <response code="200">Returns dictionary of database tables and records count. (TableName, Count)</response>
    [HttpGet("db")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetDbStatusAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.Statistics.GetTablesStatusAsync();
        return Ok(data);
    }

    /// <summary>
    /// Get statistics of database cache tables.
    /// </summary>
    /// <response code="200">Returns dictonary of row counts in database tables in the cache database. (TableName, Count)</response>
    [HttpGet("db/cache")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetDbCacheStatusAsync()
    {
        await using var cache = CacheBuilder.CreateRepository();

        var statistics = await cache.StatisticsRepository.GetTableStatisticsAsync();
        return Ok(statistics);
    }

    /// <summary>
    /// Get statistics about audit logs by type.
    /// </summary>
    /// <response code="200">Returns dictionary of audit logs statistics per type. (Type, Count)</response>
    [HttpGet("audit-log/type")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetAuditLogsStatisticsByTypeAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.AuditLog.GetStatisticsByTypeAsync();

        var result = Enum.GetValues<AuditLogItemType>()
            .Where(o => o != AuditLogItemType.None)
            .Select(o => new { Key = o.ToString(), Value = data.TryGetValue(o, out var val) ? val : 0 })
            .OrderByDescending(o => o.Value).ThenBy(o => o.Key)
            .ToDictionary(o => o.Key, o => o.Value);

        return Ok(result);
    }

    /// <summary>
    /// Get statistics about audit logs by date and year.
    /// </summary>
    /// <response code="200">Returns dictionary of audit logs statistics per date (Year-Month, Count)</response>
    [HttpGet("audit-log/date")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetAuditLogsStatisticsByDateAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.AuditLog.GetStatisticsByDateAsync();
        return Ok(data);
    }

    /// <summary>
    /// Gets statistics about commands.
    /// </summary>
    [HttpGet("commands")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StatisticItem>>> GetTextCommandStatisticsAsync()
    {
        var filterModel = new AuditLogListParams
        {
            Types = new List<AuditLogItemType> { AuditLogItemType.Command },
            IgnoreBots = true
        };

        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.AuditLog.GetSimpleDataAsync(filterModel);

        var deserializedData = data.Select(o => new
        {
            o.CreatedAt,
            Data = JsonConvert.DeserializeObject<CommandExecution>(o.Data, AuditLogWriter.SerializerSettings)
        });

        var groupedData = deserializedData.Where(o => !string.IsNullOrEmpty(o.Data!.Command))
            .GroupBy(o => o.Data.Command)
            .Select(o => new StatisticItem
            {
                Key = o.Key,
                FailedCount = o.Count(x => !x.Data!.IsSuccess),
                Last = o.Max(x => x.CreatedAt),
                SuccessCount = o.Count(x => x.Data!.IsSuccess),
                MinDuration = o.Min(x => x.Data!.Duration),
                MaxDuration = o.Max(x => x.Data!.Duration),
                TotalDuration = o.Sum(x => x.Data!.Duration)
            })
            .OrderBy(o => o.Key)
            .ToList();

        return Ok(groupedData);
    }

    /// <summary>
    /// Gets statistics about interactions.
    /// </summary>
    [HttpGet("interactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StatisticItem>>> GetInteractionsStatusAsync()
    {
        var filterModel = new AuditLogListParams
        {
            Types = new List<AuditLogItemType> { AuditLogItemType.InteractionCommand },
            IgnoreBots = true
        };

        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.AuditLog.GetSimpleDataAsync(filterModel);

        var deserializedData = data.ConvertAll(o => new
        {
            o.CreatedAt,
            Data = JsonConvert.DeserializeObject<InteractionCommandExecuted>(o.Data, AuditLogWriter.SerializerSettings)
        });

        var groupedData = deserializedData.GroupBy(o => o.Data!.FullName)
            .Select(o => new StatisticItem
            {
                Key = o.Key,
                FailedCount = o.Count(x => !x.Data!.IsSuccess),
                Last = o.Max(x => x.CreatedAt),
                SuccessCount = o.Count(x => x.Data!.IsSuccess),
                MinDuration = o.Min(x => x.Data!.Duration),
                MaxDuration = o.Max(x => x.Data!.Duration),
                TotalDuration = o.Sum(x => x.Data!.Duration)
            }).OrderBy(o => o.Key)
            .ToList();

        return Ok(groupedData);
    }

    /// <summary>
    /// Get statistics about unverify logs by type.
    /// </summary>
    /// <response code="200">Returns dictionary of unverify logs statistics per type. (Type, Count)</response>
    [HttpGet("unverify-logs/type")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetUnverifyLogsStatisticsByOperationAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var statistics = await repository.Unverify.GetStatisticsByTypeAsync();

        var data = Enum.GetValues<UnverifyOperation>()
            .Select(o => new { Key = o.ToString(), Value = statistics.TryGetValue(o, out var val) ? val : 0 })
            .OrderByDescending(o => o.Value).ThenBy(o => o.Key)
            .ToDictionary(o => o.Key, o => o.Value);

        return Ok(data);
    }

    /// <summary>
    /// Get statistics about unverify logs by date and year.
    /// </summary>
    /// <response code="200">Returns dictionary of unverify logs statistics per date (Year-Month, Count)</response>
    [HttpGet("unverify-logs/date")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetUnverifyLogsStatisticsByDateAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.Unverify.GetStatisticsByDateAsync();
        return Ok(data);
    }

    /// <summary>
    /// Get statistics of planned background jobs.
    /// </summary>
    /// <response code="200">Returns statistics of planned jobs.</response>
    [HttpGet("jobs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StatisticItem>>> GetJobStatisticsAsync()
    {
        var filterModel = new AuditLogListParams
        {
            Types = new List<AuditLogItemType> { AuditLogItemType.JobCompleted }
        };

        await using var repository = DatabaseBuilder.CreateRepository();
        var dbData = await repository.AuditLog.GetSimpleDataAsync(filterModel);

        var data = dbData
            .Select(o => new
            {
                o.CreatedAt,
                Data = JsonConvert.DeserializeObject<JobExecutionData>(o.Data, AuditLogWriter.SerializerSettings)
            })
            .GroupBy(o => o.Data!.JobName)
            .Select(o => new StatisticItem
            {
                Key = o.Key,
                FailedCount = o.Count(x => x.Data!.WasError),
                Last = o.Max(x => x.CreatedAt),
                MaxDuration = o.Max(x => Convert.ToInt32((x.Data!.EndAt - x.Data.StartAt).TotalMilliseconds)),
                MinDuration = o.Min(x => Convert.ToInt32((x.Data!.EndAt - x.Data.StartAt).TotalMilliseconds)),
                SuccessCount = o.Count(x => !x.Data!.WasError),
                TotalDuration = o.Sum(x => Convert.ToInt32((x.Data!.EndAt - x.Data.StartAt).TotalMilliseconds))
            })
            .OrderBy(o => o.Key)
            .ToList();

        return Ok(data);
    }

    /// <summary>
    /// Get statistics about API by date and year.
    /// </summary>
    /// <response code="200">Returns dictionary of api requests per date (Year-Month, Count).</response>
    [HttpGet("api/date")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetApiRequestsByDateAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.AuditLog.GetApiRequestsByDateAsync();
        return Ok(data);
    }

    /// <summary>
    /// Get statistics about API by endpoint.
    /// </summary>
    /// <response code="200">Returns statistics by endpoint.</response>
    [HttpGet("api/endpoint")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StatisticItem>>> GetApiRequestsByEndpointAsync()
    {
        var filterModel = new AuditLogListParams { Types = new List<AuditLogItemType> { AuditLogItemType.Api } };
        await using var repository = DatabaseBuilder.CreateRepository();
        var dbData = await repository.AuditLog.GetSimpleDataAsync(filterModel);

        var data = dbData
            .Select(o => new
            {
                o.CreatedAt,
                Data = JsonConvert.DeserializeObject<ApiRequest>(o.Data, AuditLogWriter.SerializerSettings)
            })
            .Where(o => !string.IsNullOrEmpty(o.Data!.StatusCode))
            .GroupBy(o => $"{o.Data.Method} {o.Data.TemplatePath}")
            .Select(o => new StatisticItem
            {
                Key = o.Key,
                FailedCount = o.Count(x => Convert.ToInt32(x.Data!.StatusCode.Split(' ')[0]) >= 400),
                Last = o.Max(x => x.CreatedAt),
                MaxDuration = o.Max(x => Convert.ToInt32((x.Data!.EndAt - x.Data.StartAt).TotalMilliseconds)),
                MinDuration = o.Min(x => Convert.ToInt32((x.Data!.EndAt - x.Data.StartAt).TotalMilliseconds)),
                SuccessCount = o.Count(x => Convert.ToInt32(x.Data!.StatusCode.Split(' ')[0]) < 400),
                TotalDuration = o.Sum(x => Convert.ToInt32((x.Data!.EndAt - x.Data.StartAt).TotalMilliseconds))
            })
            .OrderBy(o => o.Key)
            .ToList();

        return Ok(data);
    }

    /// <summary>
    /// Get statistics about API by status code.
    /// </summary>
    /// <response code="200">Returns dictionary of api requests per status code (Status code, Count).</response>
    [HttpGet("api/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetApiRequestsByStatusCodeAsync()
    {
        var filterModel = new AuditLogListParams { Types = new List<AuditLogItemType> { AuditLogItemType.Api } };
        await using var repository = DatabaseBuilder.CreateRepository();
        var dbData = await repository.AuditLog.GetSimpleDataAsync(filterModel);

        var data = dbData
            .Select(o => new
            {
                o.CreatedAt,
                Data = JsonConvert.DeserializeObject<ApiRequest>(o.Data, AuditLogWriter.SerializerSettings)
            })
            .Where(o => !string.IsNullOrEmpty(o.Data!.StatusCode))
            .GroupBy(o => o.Data.StatusCode)
            .Select(o => new { o.Key, Count = o.Count() })
            .OrderBy(o => o.Key)
            .ToDictionary(o => o.Key, o => o.Count);

        return Ok(data);
    }
}
