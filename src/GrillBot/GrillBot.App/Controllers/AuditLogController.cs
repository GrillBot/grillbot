using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.FileStorage;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.Common;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/auditlog")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [OpenApiTag("Audit log", Description = "Logging")]
    public class AuditLogController : Controller
    {
        private AuditLogService AuditLogService { get; }
        private GrillBotContext DbContext { get; }
        private FileStorageFactory FileStorageFactory { get; }

        public AuditLogController(AuditLogService auditLogService, GrillBotContext dbContext, FileStorageFactory fileStorageFactory)
        {
            AuditLogService = auditLogService;
            DbContext = dbContext;
            FileStorageFactory = fileStorageFactory;
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
                .Include(o => o.ProcessedGuildUser).ThenInclude(o => o.User)
                .Include(o => o.ProcessedUser)
                .AsSplitQuery().AsQueryable();

            query = parameters.CreateQuery(query);
            var result = await PaginatedResponse<AuditLogListItem>.CreateAsync(query, parameters, entity => new(entity, AuditLogService.JsonSerializerSettings));
            return Ok(result);
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
