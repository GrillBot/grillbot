using GrillBot.App.Services;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.AutoReply;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/autoreply")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [OpenApiTag("AutoReply", Description = "Auto response on discord messages")]
    public class AutoReplyController : Controller
    {
        private AutoReplyService AutoReplyService { get; }
        private GrillBotContext DbContext { get; }

        public AutoReplyController(AutoReplyService autoReplyService, GrillBotContext dbContext)
        {
            AutoReplyService = autoReplyService;
            DbContext = dbContext;
        }

        /// <summary>
        /// Gets nonpaginated list of auto replies.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet]
        [OpenApiOperation(nameof(AutoReplyController) + "_" + nameof(GetAutoReplyListAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AutoReplyItem>>> GetAutoReplyListAsync(CancellationToken cancellationToken)
        {
            var query = DbContext.AutoReplies.AsNoTracking()
                .OrderBy(o => o.Id);

            var data = await query.ToListAsync(cancellationToken);
            var result = data.ConvertAll(o => new AutoReplyItem(o));
            return Ok(result);
        }

        /// <summary>
        /// Gets reply item
        /// </summary>
        /// <param name="id">Reply ID</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Success</response>
        /// <response code="404">Reply not found</response>
        [HttpGet("{id}")]
        [OpenApiOperation(nameof(AutoReplyController) + "_" + nameof(GetItemAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AutoReplyItem>> GetItemAsync(long id, CancellationToken cancellationToken)
        {
            var entity = await DbContext.AutoReplies.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

            if (entity == null)
                return NotFound(new MessageResponse($"Požadovaná automatická odpověď s ID {id} nebyla nalezena."));

            return Ok(new AutoReplyItem(entity));
        }

        /// <summary>
        /// Creates new reply item.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpPost]
        [OpenApiOperation(nameof(AutoReplyController) + "_" + nameof(CreateItemAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AutoReplyItem>> CreateItemAsync(AutoReplyItemParams parameters, CancellationToken cancellationToken)
        {
            var entity = new Database.Entity.AutoReplyItem()
            {
                Flags = parameters.Flags,
                Reply = parameters.Reply,
                Template = parameters.Template
            };

            await DbContext.AddAsync(entity, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);
            await AutoReplyService.InitAsync(cancellationToken);
            return Ok(new AutoReplyItem(entity));
        }

        /// <summary>
        /// Updates existing reply item.
        /// </summary>
        /// <param name="id">Reply ID</param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Item not found</response>
        [HttpPut("{id}")]
        [OpenApiOperation(nameof(AutoReplyController) + "_" + nameof(UpdateItemAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<AutoReplyItem>> UpdateItemAsync(long id, [FromBody] AutoReplyItemParams parameters, CancellationToken cancellationToken)
        {
            var entity = await DbContext.AutoReplies.AsQueryable()
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

            if (entity == null)
                return NotFound(new MessageResponse($"Požadovaná automatická odpověď s ID {id} nebyla nalezena."));

            entity.Template = parameters.Template;
            entity.Flags = parameters.Flags;
            entity.Reply = parameters.Reply;

            await DbContext.SaveChangesAsync(cancellationToken);
            await AutoReplyService.InitAsync(cancellationToken);
            return Ok(new AutoReplyItem(entity));
        }

        /// <summary>
        /// Removes reply item
        /// </summary>
        /// <param name="id">Reply ID</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Success</response>
        /// <response code="404">Item not found</response>
        [HttpDelete("{id}")]
        [OpenApiOperation(nameof(AutoReplyController) + "_" + nameof(RemoveItemAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RemoveItemAsync(long id, CancellationToken cancellationToken)
        {
            var entity = await DbContext.AutoReplies.AsQueryable()
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

            if (entity == null)
                return NotFound(new MessageResponse($"Požadovaná automatická odpověď s ID {id} nebyla nalezena."));

            DbContext.Remove(entity);
            await DbContext.SaveChangesAsync(cancellationToken);
            await AutoReplyService.InitAsync(cancellationToken);
            return Ok();
        }
    }
}
