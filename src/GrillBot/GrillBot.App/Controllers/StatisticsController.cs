using GrillBot.App.Services.AuditLog;
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
    private GrillBotContextFactory DbFactory { get; }

    public StatisticsController(GrillBotContextFactory dbFactory)
    {
        DbFactory = dbFactory;
    }

    /// <summary>
    /// Get statistics about database tables.
    /// </summary>
    /// <response code="200">Returns dictionary of database tables and records count. (TableName, Count)</response>
    [HttpGet("db")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetDbStatusAsync(CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var data = new Dictionary<string, int>()
        {
            { nameof(context.Users), await context.Users.CountAsync(cancellationToken) },
            { nameof(context.Guilds), await context.Guilds.CountAsync(cancellationToken) },
            { nameof(context.GuildUsers), await context.GuildUsers.CountAsync(cancellationToken) },
            { nameof(context.Channels), await context.Channels.CountAsync(cancellationToken) },
            { nameof(context.UserChannels), await context.UserChannels.CountAsync(cancellationToken) },
            { nameof(context.Invites), await context.Invites.CountAsync(cancellationToken) },
            { nameof(context.SearchItems), await context.SearchItems.CountAsync(cancellationToken) },
            { nameof(context.Unverifies), await context.Unverifies.CountAsync(cancellationToken) },
            { nameof(context.UnverifyLogs), await context.UnverifyLogs.CountAsync(cancellationToken) },
            { nameof(context.AuditLogs), await context.AuditLogs.CountAsync(cancellationToken) },
            { nameof(context.AuditLogFiles), await context.AuditLogFiles.CountAsync(cancellationToken) },
            { nameof(context.Emotes), await context.Emotes.CountAsync(cancellationToken) },
            { nameof(context.Reminders), await context.Reminders.CountAsync(cancellationToken) },
            { nameof(context.SelfunverifyKeepables), await context.SelfunverifyKeepables.CountAsync(cancellationToken) },
            { nameof(context.ExplicitPermissions), await context.ExplicitPermissions.CountAsync(cancellationToken) },
            { nameof(context.AutoReplies), await context.AutoReplies.CountAsync(cancellationToken) },
            { nameof(context.MessageCacheIndexes), await context.MessageCacheIndexes.CountAsync(cancellationToken) },
            { nameof(context.Suggestions), await context.Suggestions.CountAsync(cancellationToken) }
        };

        return Ok(data);
    }

    /// <summary>
    /// Get statistics about audit logs by type.
    /// </summary>
    /// <response code="200">Returns dictionary of audit logs statistics per type. (Type, Count)</response>
    [HttpGet("audit-log/type")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetAuditLogsStatisticsByTypeAsync(CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var statistics = context.AuditLogs.AsNoTracking()
            .GroupBy(o => o.Type)
            .Select(o => new { Type = o.Key, Count = o.Count() });

        var dbData = await statistics.ToDictionaryAsync(o => o.Type, o => o.Count, cancellationToken);
        var data = Enum.GetValues<AuditLogItemType>()
            .Where(o => o > AuditLogItemType.None)
            .ToDictionary(o => o.ToString(), o => dbData.TryGetValue(o, out int val) ? val : 0);
        return Ok(data);
    }

    /// <summary>
    /// Get statistics about audit logs by date and year.
    /// </summary>
    /// <response code="200">Returns dictionary of audit logs statistics per date (Year-Month, Count)</response>
    [HttpGet("audit-log/date")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetAuditLogsStatisticsByDateAsync(CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var query = context.AuditLogs.AsNoTracking()
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .OrderByDescending(o => o.Key.Year).ThenByDescending(o => o.Key.Month)
            .Select(o => new { Date = $"{o.Key.Year}-{o.Key.Month}", Count = o.Count() });

        var data = await query.ToDictionaryAsync(o => o.Date, o => o.Count, cancellationToken);
        return Ok(data);
    }

    /// <summary>
    /// Gets statistics about commands.
    /// </summary>
    [HttpGet("commands")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StatisticItem>>> GetTextCommandStatisticsAsync(CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var query = context.AuditLogs.AsNoTracking()
            .Where(o => o.Type == AuditLogItemType.Command)
            .Select(o => new { o.CreatedAt, o.Data });

        var dbData = await query.ToListAsync(cancellationToken);
        var deserializedData = dbData.Select(o => new
        {
            o.CreatedAt,
            Data = JsonConvert.DeserializeObject<CommandExecution>(o.Data, AuditLogService.JsonSerializerSettings)
        });

        var groupedData = deserializedData.Where(o => !string.IsNullOrEmpty(o.Data.Command))
            .GroupBy(o => o.Data.Command)
            .Select(o => new StatisticItem()
            {
                Key = o.Key,
                FailedCount = o.Count(x => !x.Data.IsSuccess),
                Last = o.Max(x => x.CreatedAt),
                SuccessCount = o.Count(x => x.Data.IsSuccess),
                MinDuration = o.Min(x => x.Data.Duration),
                MaxDuration = o.Max(x => x.Data.Duration),
                TotalDuration = o.Sum(x => x.Data.Duration)
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
    public async Task<ActionResult<List<StatisticItem>>> GetInteractionsStatusAsync(CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var query = context.AuditLogs.AsNoTracking()
            .Where(o => o.Type == AuditLogItemType.InteractionCommand)
            .Select(o => new { o.CreatedAt, o.Data });

        var dbData = await query.ToListAsync(cancellationToken);
        var deserializedData = dbData.ConvertAll(o => new
        {
            o.CreatedAt,
            Data = JsonConvert.DeserializeObject<InteractionCommandExecuted>(o.Data, AuditLogService.JsonSerializerSettings)
        });

        var groupedData = deserializedData.GroupBy(o => o.Data.FullName)
            .Select(o => new StatisticItem()
            {
                Key = o.Key,
                FailedCount = o.Count(x => !x.Data.IsSuccess),
                Last = o.Max(x => x.CreatedAt),
                SuccessCount = o.Count(x => x.Data.IsSuccess),
                MinDuration = o.Min(x => x.Data.Duration),
                MaxDuration = o.Max(x => x.Data.Duration),
                TotalDuration = o.Sum(x => x.Data.Duration)
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
    public async Task<ActionResult<Dictionary<string, int>>> GetUnverifyLogsStatisticsByOperationAsync(CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var statistics = context.UnverifyLogs.AsNoTracking()
            .GroupBy(o => o.Operation)
            .Select(o => new { Type = o.Key, Count = o.Count() });

        var dbData = await statistics.ToDictionaryAsync(o => o.Type, o => o.Count, cancellationToken);
        var data = Enum.GetValues<UnverifyOperation>()
            .ToDictionary(o => o.ToString(), o => dbData.TryGetValue(o, out int val) ? val : 0);
        return Ok(data);
    }

    /// <summary>
    /// Get statistics about unverify logs by date and year.
    /// </summary>
    /// <response code="200">Returns dictionary of unverify logs statistics per date (Year-Month, Count)</response>
    [HttpGet("unverify-logs/date")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetUnverifyLogsStatisticsByDateAsync(CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var query = context.UnverifyLogs.AsNoTracking()
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .OrderByDescending(o => o.Key.Year).ThenByDescending(o => o.Key.Month)
            .Select(o => new { Date = $"{o.Key.Year}-{o.Key.Month}", Count = o.Count() });

        var data = await query.ToDictionaryAsync(o => o.Date, o => o.Count, cancellationToken);
        return Ok(data);
    }

    /// <summary>
    /// Get statistics of planned background jobs.
    /// </summary>
    /// <response code="200">Returns statistics of planned jobs.</response>
    [HttpGet("jobs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StatisticItem>>> GetJobStatisticsAsync(CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var query = context.AuditLogs.AsNoTracking()
            .Where(o => o.Type == AuditLogItemType.JobCompleted)
            .Select(o => new { o.CreatedAt, o.Data });

        var dbData = await query.ToListAsync(cancellationToken);

        var data = dbData
            .Select(o => new
            {
                o.CreatedAt,
                Data = JsonConvert.DeserializeObject<JobExecutionData>(o.Data, AuditLogService.JsonSerializerSettings)
            })
            .GroupBy(o => o.Data.JobName)
            .Select(o => new StatisticItem()
            {
                Key = o.Key,
                FailedCount = o.Count(x => x.Data.WasError),
                Last = o.Max(x => x.CreatedAt),
                MaxDuration = o.Max(x => Convert.ToInt32((x.Data.EndAt - x.Data.StartAt).TotalMilliseconds)),
                MinDuration = o.Min(x => Convert.ToInt32((x.Data.EndAt - x.Data.StartAt).TotalMilliseconds)),
                SuccessCount = o.Count(x => !x.Data.WasError),
                TotalDuration = o.Sum(x => Convert.ToInt32((x.Data.EndAt - x.Data.StartAt).TotalMilliseconds))
            })
            .OrderBy(o => o.Key)
            .ToList();

        return Ok(data);
    }
}
