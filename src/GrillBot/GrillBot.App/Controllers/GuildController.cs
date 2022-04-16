using GrillBot.App.Services;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Guilds;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/guild")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [OpenApiTag("Guilds", Description = "Guild management")]
    public class GuildController : Controller
    {
        private GrillBotContext DbContext { get; }
        private DiscordSocketClient DiscordClient { get; }
        private GuildService GuildService { get; }

        public GuildController(GrillBotContext dbContext, DiscordSocketClient discordClient, GuildService guildService)
        {
            DbContext = dbContext;
            DiscordClient = discordClient;
            GuildService = guildService;
        }

        /// <summary>
        /// Gets paginated list of guilds.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet]
        [OpenApiOperation(nameof(GuildController) + "_" + nameof(GetGuildListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<PaginatedResponse<Guild>>> GetGuildListAsync([FromQuery] GetGuildListParams parameters, CancellationToken cancellationToken)
        {
            var query = parameters.CreateQuery(
                DbContext.Guilds.AsNoTracking().OrderBy(o => o.Name)
            );
            var result = await PaginatedResponse<Guild>.CreateAsync(query, parameters, entity => new Guild(entity), cancellationToken);

            foreach (var guildData in result.Data)
            {
                var guild = DiscordClient.GetGuild(Convert.ToUInt64(guildData.Id));
                if (guild == null) continue;

                guildData.MemberCount = guild.MemberCount;
                guildData.IsConnected = guild.IsConnected;
            }

            return Ok(result);
        }

        /// <summary>
        /// Gets detailed information about guild.
        /// </summary>
        /// <param name="id">Guild ID</param>
        /// <param name="cancellationToken"></param>
        [HttpGet("{id}")]
        [OpenApiOperation(nameof(GuildController) + "_" + nameof(GetGuildDetailAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<GuildDetail>> GetGuildDetailAsync(ulong id, CancellationToken cancellationToken)
        {
            var guildDetail = await GuildService.GetGuildDetailAsync(id, cancellationToken);
            if (guildDetail == null)
                return NotFound(new MessageResponse("Nepodařilo se dohledat server."));

            return Ok(guildDetail);
        }

        /// <summary>
        /// Updates guild
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">Guild not found.</response>
        [HttpPut("{id}")]
        [OpenApiOperation(nameof(GuildController) + "_" + nameof(GetGuildDetailAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<GuildDetail>> UpdateGuildAsync(ulong id, [FromBody] UpdateGuildParams updateGuildParams, CancellationToken cancellationToken)
        {
            var guild = DiscordClient.GetGuild(id);

            if (guild == null)
                return NotFound(new MessageResponse("Nepodařilo se dohledat server."));

            var dbGuild = await DbContext.Guilds.AsQueryable()
                .FirstOrDefaultAsync(o => o.Id == id.ToString(), cancellationToken);

            if (updateGuildParams.AdminChannelId != null && guild.GetTextChannel(Convert.ToUInt64(updateGuildParams.AdminChannelId)) == null)
                ModelState.AddModelError(nameof(updateGuildParams.AdminChannelId), "Nepodařilo se dohledat administrátorský kanál");
            else
                dbGuild.AdminChannelId = updateGuildParams.AdminChannelId;

            if (updateGuildParams.MuteRoleId != null && guild.GetRole(Convert.ToUInt64(updateGuildParams.MuteRoleId)) == null)
                ModelState.AddModelError(nameof(updateGuildParams.MuteRoleId), "Nepodařilo se dohledat roli, která reprezentuje umlčení uživatele při unverify.");
            else
                dbGuild.MuteRoleId = updateGuildParams.MuteRoleId;

            if (updateGuildParams.EmoteSuggestionChannelId != null && guild.GetTextChannel(Convert.ToUInt64(updateGuildParams.EmoteSuggestionChannelId)) == null)
                ModelState.AddModelError(nameof(updateGuildParams.EmoteSuggestionChannelId), "Nepodařilo se dohledat kanál pro návrhy emotů.");
            else
                dbGuild.EmoteSuggestionChannelId = updateGuildParams.EmoteSuggestionChannelId;

            if (!ModelState.IsValid)
                return BadRequest(new ValidationProblemDetails(ModelState));

            await DbContext.SaveChangesAsync(cancellationToken);
            return Ok(await GuildService.GetGuildDetailAsync(id, cancellationToken));
        }
    }
}
