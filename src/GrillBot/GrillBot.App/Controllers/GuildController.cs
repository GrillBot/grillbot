using GrillBot.App.Services.Guild;
using GrillBot.Data.Extensions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Guilds;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/guild")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[OpenApiTag("Guilds", Description = "Guild management")]
public class GuildController : Controller
{
    private GuildApiService ApiService { get; }

    public GuildController(GuildApiService apiService)
    {
        ApiService = apiService;
    }

    /// <summary>
    /// Get paginated list of guilds.
    /// </summary>
    /// <response code="200">Return paginated list of guilds in DB.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PaginatedResponse<Guild>>> GetGuildListAsync([FromQuery] GetGuildListParams parameters, CancellationToken cancellationToken)
    {
        var result = await ApiService.GetListAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets detailed information about guild.
    /// </summary>
    /// <param name="id">Guild ID</param>
    /// <param name="cancellationToken"></param>
    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<GuildDetail>> GetGuildDetailAsync(ulong id, CancellationToken cancellationToken)
    {
        var guildDetail = await ApiService.GetDetailAsync(id, cancellationToken);
        if (guildDetail == null)
            return NotFound(new MessageResponse("Nepodařilo se dohledat server."));

        return Ok(guildDetail);
    }

    /// <summary>
    /// Updates guild
    /// </summary>
    /// <response code="200">Return guild detail.</response>
    /// <response code="400">Validation of parameters failed.</response>
    /// <response code="404">Guild not found.</response>
    [HttpPut("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<GuildDetail>> UpdateGuildAsync(ulong id, [FromBody] UpdateGuildParams parameters)
    {
        this.SetApiRequestData(parameters);
        var result = await ApiService.UpdateGuildAsync(id, parameters, ModelState);

        if (result == null)
        {
            return NotFound(new MessageResponse("Nepodařilo se dohledat server."));
        }
        else if (!ModelState.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        return Ok(result);
    }
}
