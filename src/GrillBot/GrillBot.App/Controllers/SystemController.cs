using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.API.System;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using YamlDotNet.Serialization.NamingConventions;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/system")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [OpenApiTag("System", Description = "Internal system management, ...")]
    public class SystemController : Controller
    {
        private IWebHostEnvironment Environment { get; }
        private DiscordSocketClient DiscordClient { get; }
        private GrillBotContext DbContext { get; }
        private DiscordInitializationService InitializationService { get; }

        public SystemController(IWebHostEnvironment environment, DiscordSocketClient discordClient,
            GrillBotContext dbContext, DiscordInitializationService initializationService)
        {
            Environment = environment;
            DiscordClient = discordClient;
            DbContext = dbContext;
            InitializationService = initializationService;
        }

        /// <summary>
        /// Gets diagnostics data about application.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("diag")]
        [OpenApiOperation(nameof(SystemController) + "_" + nameof(GetDiagnostics))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<DiagnosticsInfo> GetDiagnostics()
        {
            var data = new DiagnosticsInfo(Environment.EnvironmentName, DiscordClient, InitializationService.Get());
            return Ok(data);
        }

        /// <summary>
        /// Gets infromation about database tables.
        /// </summary>
        [HttpGet("db")]
        [OpenApiOperation(nameof(SystemController) + "_" + nameof(GetDbStatusAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<Dictionary<string, int>>> GetDbStatusAsync(CancellationToken cancellationToken)
        {
            var data = new Dictionary<string, int>()
            {
                { nameof(DbContext.Users), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Users, cancellationToken) },
                { nameof(DbContext.Guilds), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Guilds, cancellationToken) },
                { nameof(DbContext.GuildUsers), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.GuildUsers, cancellationToken) },
                { nameof(DbContext.Channels), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Channels, cancellationToken) },
                { nameof(DbContext.UserChannels), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.UserChannels, cancellationToken) },
                { nameof(DbContext.Invites), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Invites, cancellationToken) },
                { nameof(DbContext.SearchItems), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.SearchItems, cancellationToken) },
                { nameof(DbContext.Unverifies), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Unverifies, cancellationToken) },
                { nameof(DbContext.UnverifyLogs), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.UnverifyLogs, cancellationToken) },
                { nameof(DbContext.AuditLogs), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.AuditLogs, cancellationToken) },
                { nameof(DbContext.AuditLogFiles), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.AuditLogFiles, cancellationToken) },
                { nameof(DbContext.Emotes), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Emotes, cancellationToken) },
                { nameof(DbContext.Reminders), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Reminders, cancellationToken) },
                { nameof(DbContext.SelfunverifyKeepables), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.SelfunverifyKeepables, cancellationToken) },
                { nameof(DbContext.ExplicitPermissions), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.ExplicitPermissions, cancellationToken) },
                { nameof(DbContext.AutoReplies), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.AutoReplies, cancellationToken) },
                { nameof(DbContext.MessageCacheIndexes), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.MessageCacheIndexes, cancellationToken) }
            };

            return Ok(data);
        }

        /// <summary>
        /// Gets information about audit logs statistics.
        /// </summary>
        [HttpGet("db/audit-log")]
        [OpenApiOperation(nameof(SystemController) + "_" + nameof(GetAuditLogsStatisticsAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Dictionary<string, int>>> GetAuditLogsStatisticsAsync(CancellationToken cancellationToken)
        {
            var statistics = DbContext.AuditLogs.AsNoTracking()
                .GroupBy(o => o.Type)
                .Select(o => new { Type = o.Key, Count = o.Count() });

            var dbData = await statistics.ToDictionaryAsync(o => o.Type, o => o.Count, cancellationToken);
            var data = Enum.GetValues<AuditLogItemType>()
                .Where(o => o > AuditLogItemType.None)
                .ToDictionary(o => o.ToString(), o => dbData.TryGetValue(o, out int val) ? val : 0);
            return Ok(data);
        }

        /// <summary>
        /// Gets statistics about commands.
        /// </summary>
        [HttpGet("commands")]
        [OpenApiOperation(nameof(SystemController) + "_" + nameof(GetCommandStatusAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CommandStatisticItem>>> GetCommandStatusAsync(string searchQuery = null,
            CancellationToken cancellationToken = default)
        {
            var query = DbContext.AuditLogs.AsNoTracking()
                .Where(o => o.Type == AuditLogItemType.Command)
                .Select(o => new { o.CreatedAt, o.Data });

            var dbData = await query.ToListAsync(cancellationToken);
            var deserializedData = dbData.ConvertAll(o => new
            {
                o.CreatedAt,
                Data = JsonConvert.DeserializeObject<CommandExecution>(o.Data, AuditLogService.JsonSerializerSettings)
            });

            var dataQuery = deserializedData.Where(o => o.Data.Command != null);
            if (!string.IsNullOrEmpty(searchQuery))
                dataQuery = dataQuery.Where(o => o.Data.Command.Contains(searchQuery));

            var groupedData = dataQuery
                .GroupBy(o => o.Data.Command)
                .Select(o => new CommandStatisticItem()
                {
                    Command = o.Key,
                    FailedCount = o.Count(x => !x.Data.IsSuccess),
                    LastCall = o.Max(x => x.CreatedAt),
                    SuccessCount = o.Count(x => x.Data.IsSuccess)
                })
                .OrderBy(o => o.Command)
                .ToList();

            return Ok(groupedData);
        }

        /// <summary>
        /// Gets statistics about interactions.
        /// </summary>
        [HttpGet("interactions")]
        [OpenApiOperation(nameof(SystemController) + "_" + nameof(GetInteractionsStatusAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CommandStatisticItem>>> GetInteractionsStatusAsync(string searchQuery = null,
            CancellationToken cancellationToken = default)
        {
            var query = DbContext.AuditLogs.AsNoTracking()
                .Where(o => o.Type == AuditLogItemType.InteractionCommand)
                .Select(o => new { o.CreatedAt, o.Data });

            var dbData = await query.ToListAsync(cancellationToken);
            var deserializedData = dbData.ConvertAll(o => new
            {
                o.CreatedAt,
                Data = JsonConvert.DeserializeObject<InteractionCommandExecuted>(o.Data, AuditLogService.JsonSerializerSettings)
            });

            var dataQuery = deserializedData.AsEnumerable();
            if (!string.IsNullOrEmpty(searchQuery))
                dataQuery = dataQuery.Where(o => o.Data.FullName.Contains(searchQuery));

            var groupedData = dataQuery.GroupBy(o => o.Data.FullName)
                .Select(o => new CommandStatisticItem()
                {
                    Command = o.Key,
                    FailedCount = o.Count(x => !x.Data.IsSuccess),
                    LastCall = o.Max(x => x.CreatedAt),
                    SuccessCount = o.Count(x => x.Data.IsSuccess)
                }).OrderBy(o => o.Command)
                .ToList();

            return Ok(groupedData);

        }

        /// <summary>
        /// Changes bot account status and set bot's status activity.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpPut("status")]
        [OpenApiOperation(nameof(SystemController) + "_" + nameof(ChangeBotStatus))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult ChangeBotStatus(bool isActive)
        {
            InitializationService.Set(isActive);
            return Ok();
        }
    }
}
