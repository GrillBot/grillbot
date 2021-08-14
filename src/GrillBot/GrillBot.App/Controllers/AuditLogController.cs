using Discord;
using Discord.WebSocket;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.FileStorage;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.Common;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/auditlog")]
    [OpenApiTag("Audit log", Description = "Logging")]
    public class AuditLogController : Controller
    {
        private AuditLogService AuditLogService { get; }
        private DiscordSocketClient DiscordClient { get; }
        private GrillBotContext DbContext { get; }
        private FileStorageFactory FileStorageFactory { get; }

        public AuditLogController(AuditLogService auditLogService, DiscordSocketClient discordClient, GrillBotContext dbContext,
            FileStorageFactory fileStorageFactory)
        {
            AuditLogService = auditLogService;
            DiscordClient = discordClient;
            DbContext = dbContext;
            FileStorageFactory = fileStorageFactory;
        }

        /// <summary>
        /// Gets nonpaginated list of audit log statistics by type.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("stats/type")]
        [OpenApiOperation(nameof(AuditLogController) + "_" + nameof(GetStatisticsByTypeAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<AuditLogStatItem>>> GetStatisticsByTypeAsync()
        {
            var data = await AuditLogService.GetStatisticsAsync(o => o.Type, data =>
            {
                return Enum.GetValues<AuditLogItemType>().Select(o =>
                {
                    var item = data.Find(x => x.Item1 == o);
                    return new AuditLogStatItem(o.GetDisplayName(), item?.Item2, item?.Item3, item?.Item4);
                });
            });

            return Ok(data);
        }

        /// <summary>
        /// Gets nonpaginated list of audit log statistics by guild.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("stats/guild")]
        [OpenApiOperation(nameof(AuditLogController) + "_" + nameof(GetStatisticsByGuildAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<AuditLogStatItem>>> GetStatisticsByGuildAsync()
        {
            var data = await AuditLogService.GetStatisticsAsync(o => o.GuildId, data =>
            {
                var result = DiscordClient.Guilds.Select(g =>
                {
                    var item = data.Find(o => o.Item1 == g.Id.ToString());
                    return new AuditLogStatItem(g.Name, item?.Item2, item?.Item3, item?.Item4);
                }).ToList();

                var unknown = data.Find(o => o.Item1 == null);
                if (unknown != null)
                    result.Add(new AuditLogStatItem("Neznámý server", unknown?.Item2, unknown?.Item3, unknown?.Item4));

                return result;
            });

            return Ok(data);
        }

        /// <summary>
        /// Gets nonpaginated list of audit log statistics by channel.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("stats/channel")]
        [OpenApiOperation(nameof(AuditLogController) + "_" + nameof(GetStatisticsByChannelAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<AuditLogStatItem>>> GetStatisticsByChannelAsync()
        {
            var data = await AuditLogService.GetStatisticsAsync(o => o.ChannelId, data =>
            {
                return data.Select(o =>
                {
                    if (string.IsNullOrEmpty(o.Item1) || DiscordClient.GetChannel(Convert.ToUInt64(o.Item1)) is not IChannel channel)
                        return new AuditLogStatItem("Neznámý kanál", o.Item2, o.Item3, o.Item4);

                    return new AuditLogStatItem(channel.Name, o.Item2, o.Item3, o.Item4);
                }).Where(o => o != null).OrderBy(o => o.StatName == "Neznámý kanál" ? new string('z', byte.MaxValue) : o.StatName).ThenBy(o => o.Count);
            });

            return Ok(data);
        }

        /// <summary>
        /// Gets nonpaginated list of audit log statistics by user.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("stats/user")]
        [OpenApiOperation(nameof(AuditLogController) + "_" + nameof(GetStatisticsByUserAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<AuditLogStatItem>>> GetStatisticsByUserAsync()
        {
            foreach (var guild in DiscordClient.Guilds)
                await guild.DownloadUsersAsync();

            var data = await AuditLogService.GetStatisticsAsync(o => o.ProcessedUserId, data =>
            {
                return data.Select(o =>
                {
                    var user = DiscordClient.GetUser(Convert.ToUInt64(o.Item1));

                    if (string.IsNullOrEmpty(o.Item1) || user == null)
                        return new AuditLogStatItem($"Neznámý uživatel {(string.IsNullOrEmpty(o.Item1) ? "" : $"({o.Item1})")}", o.Item2, o.Item3, o.Item4);

                    return new AuditLogStatItem(user.GetFullName(), o.Item2, o.Item3, o.Item4);
                }).Where(o => o != null).OrderBy(o => o.StatName).ThenBy(o => o.Count);
            });

            return Ok(data);
        }

        /// <summary>
        /// Gets list of years of logs.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("stats/availableYears")]
        [OpenApiOperation(nameof(AuditLogController) + "_" + nameof(GetAvailableLogYearsAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<int>>> GetAvailableLogYearsAsync()
        {
            // TODO: Test
            var data = await AuditLogService.GetAvailableLogYearsAsync();
            return Ok(data);
        }

        /// <summary>
        /// Gets nonpaginated list of audit logs statistics by month in year.
        /// </summary>
        /// <param name="year">Year</param>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet("stats/months/{year}")]
        [OpenApiOperation(nameof(AuditLogController) + "_" + nameof(GetStatisticsByMonthAtYearAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<List<AuditLogStatItem>>> GetStatisticsByMonthAtYearAsync(int year)
        {
            var data = await AuditLogService.GetStatisticsByMonthAtYearAsync(year);
            return Ok(data);
        }

        /// <summary>
        /// Removes item from log.
        /// </summary>
        /// <param name="id">Log item ID</param>
        /// <response code="200">Success</response>
        /// <response code="404">Item not found.</response>
        [HttpDelete("{id}")]
        [OpenApiOperation(nameof(AuditLogController) + "_" + nameof(RemoveItemAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> RemoveItemAsync(long id)
        {
            // TODO: Test
            var result = await AuditLogService.RemoveItemAsync(id);

            if (!result)
                return NotFound(new MessageResponse("Požadovaný záznam v logu nebyl nalezen nebo nemáš oprávnění přistoupit k tomuto záznamu."));

            return Ok();
        }

        /// <summary>
        /// Gets paginated list of audit logs.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed.</response>
        [HttpGet]
        [OpenApiOperation(nameof(AuditLogController) + "_" + nameof(GetAuditLogListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<PaginatedResponse<AuditLogListItem>>> GetAuditLogListAsync([FromQuery] AuditLogListParams parameters)
        {
            var query = DbContext.AuditLogs.AsNoTracking()
                .Include(o => o.Files)
                .Include(o => o.Guild)
                .Include(o => o.GuildChannel)
                .Include(o => o.ProcessedGuildUser)
                .ThenInclude(o => o.User)
                .AsQueryable();

            query = parameters.CreateQuery(query);
            var result = await PaginatedResponse<AuditLogListItem>.CreateAsync(query, parameters, entity => new(entity));
            return Ok(result);
        }

        /// <summary>
        /// Gets data of operation.
        /// </summary>
        /// <param name="id">Log ID</param>
        /// <response code="200">Success</response>
        /// <response code="204">Item not have data.</response>
        /// <response code="404">Item not found.</response>
        [HttpGet("{id}")]
        [OpenApiOperation(nameof(AuditLogController) + "_" + nameof(GetAuditLogDataAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<object>> GetAuditLogDataAsync(long id)
        {
            var item = await DbContext.AuditLogs.AsNoTracking()
                .Select(o => new { o.Id, o.Type, o.Data })
                .FirstOrDefaultAsync(o => o.Id == id);

            if (item == null)
                return NotFound(new MessageResponse("Požadovaný záznam v logu nebyl nalezen nebo nemáš oprávnění přistoupit k tomuto záznamu."));

            if (string.IsNullOrEmpty(item.Data))
                return NoContent();

            switch (item.Type)
            {
                case AuditLogItemType.Error:
                case AuditLogItemType.Info:
                case AuditLogItemType.Warning:
                    return Ok(item.Data);
                default:
                    var obj = JsonConvert.DeserializeObject(item.Data);
                    return Ok(obj);
            }

        }

        /// <summary>
        /// Gets file that stored in log.
        /// </summary>
        /// <response code="200">Success.</response>
        /// <response code="404">Item not found or file not exists.</response>
        [HttpGet("{id}/{fileId}")]
        [OpenApiOperation(nameof(AuditLogController) + "_" + nameof(GetFileContentAsync))]
        [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetFileContentAsync(long id, long fileId)
        {
            var logItem = await DbContext.AuditLogs.AsNoTracking()
                .Select(o => new
                {
                    o.Id,
                    File = o.Files.FirstOrDefault(x => x.Id == fileId)
                })
                .FirstOrDefaultAsync(o => o.Id == id);

            if (logItem == null)
                return NotFound(new MessageResponse("Požadovaný záznam v logu nebyl nalezen nebo nemáš oprávnění přistoupit k tomuto záznamu."));

            if (logItem.File == null)
                return NotFound(new MessageResponse("K tomuto záznamu neexistuje žádný záznam o existenci souboru."));

            var storage = FileStorageFactory.Create("Audit");
            var file = await storage.GetFileInfoAsync("DeletedAttachments", logItem.File.Filename);

            if (!file.Exists)
                return NotFound(new MessageResponse("Hledaný soubor neexistuje na disku."));

            var contentType = new FileExtensionContentTypeProvider()
                .TryGetContentType(file.FullName, out var _contentType) ? _contentType : "application/octet-stream";

            var bytes = System.IO.File.ReadAllBytes(file.FullName);
            return File(bytes, contentType);
        }
    }
}
