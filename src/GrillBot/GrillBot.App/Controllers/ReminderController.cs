using GrillBot.App.Services.Reminder;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Reminder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/remind")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [OpenApiTag("Reminder", Description = "Reminder management")]
    public class ReminderController : Controller
    {
        private GrillBotContext DbContext { get; }
        private RemindService RemindService { get; }
        private DiscordSocketClient DiscordClient { get; }

        public ReminderController(GrillBotContext dbContext, RemindService remindService,
            DiscordSocketClient discordClient)
        {
            DbContext = dbContext;
            RemindService = remindService;
            DiscordClient = discordClient;
        }

        /// <summary>
        /// Gets paginated list of reminders.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet]
        [OpenApiOperation(nameof(ReminderController) + "_" + nameof(GetRemindMessagesListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<PaginatedResponse<RemindMessage>>> GetRemindMessagesListAsync([FromQuery] GetReminderListParams parameters,
            CancellationToken cancellationToken)
        {
            var query = DbContext.Reminders.AsNoTracking()
                .Include(o => o.FromUser)
                .Include(o => o.ToUser)
                .AsQueryable();

            query = parameters.CreateQuery(query);
            var result = await PaginatedResponse<RemindMessage>.CreateAsync(query, parameters, entity => new(entity), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Cancels reminder.
        /// </summary>
        /// <param name="id">Remind ID</param>
        /// <param name="notify">Send notification before cancel.</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Success</response>
        /// <response code="404">Remind not found.</response>
        /// <response code="410">Remind was notified or cancelled.</response>
        [HttpDelete("{id}")]
        [OpenApiOperation(nameof(ReminderController) + "_" + nameof(CancelRemindAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.Gone)]
        public async Task<ActionResult> CancelRemindAsync(long id, [FromQuery] bool notify = false, CancellationToken cancellationToken = default)
        {
            try
            {
                await RemindService.ServiceCancellationAsync(id, User, notify, cancellationToken);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new MessageResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode((int)HttpStatusCode.Gone, new MessageResponse(ex.Message));
            }
        }
    }
}
