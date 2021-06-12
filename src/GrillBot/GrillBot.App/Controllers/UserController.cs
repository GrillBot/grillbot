using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Params;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/user")]
    [OpenApiTag("Users", Description = "Users management")]
    public class UserController : Controller
    {
        private GrillBotContext Context { get; }

        public UserController(GrillBotContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Updates user note.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="parameters">Parameters</param>
        /// <response code="200">OK</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">User not found</response>
        [HttpPost("{userId}/note")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> UpdateUserNoteAsync(ulong userId, UpdateUserNoteParams parameters)
        {
            var user = await Context.Users.FirstOrDefaultAsync(o => o.Id == userId.ToString());

            if (user == null)
                return NotFound(new MessageResponse($"Požadovaný uživatel s ID {userId} nebyl nalezen."));

            user.Note = parameters.Content;
            await Context.SaveChangesAsync();
            return Ok();
        }
    }
}
