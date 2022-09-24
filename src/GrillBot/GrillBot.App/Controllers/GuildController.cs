using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions;
using GrillBot.App.Services.Guild;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/guild")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class GuildController : Controller
{
    private GuildApiService ApiService { get; }
    private IServiceProvider ServiceProvider { get; }

    public GuildController(GuildApiService apiService, IServiceProvider serviceProvider)
    {
        ApiService = apiService;
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Get paginated list of guilds.
    /// </summary>
    /// <response code="200">Return paginated list of guilds in DB.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("list")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PaginatedResponse<Guild>>> GetGuildListAsync([FromBody] GetGuildListParams parameters)
    {
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Guild.GetGuildList>();
        var result = await action.ProcessAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Get detailed information about guild.
    /// </summary>
    /// <param name="id">Guild ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<GuildDetail>> GetGuildDetailAsync(ulong id)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Guild.GetGuildDetail>();
        var result = await action.ProcessAsync(id);

        return Ok(result);
    }

    /// <summary>
    /// Update guild
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
        this.StoreParameters(parameters);
        var result = await ApiService.UpdateGuildAsync(id, parameters, ModelState);

        if (result == null)
            return NotFound(new MessageResponse("Nepodařilo se dohledat server."));

        if (!ModelState.IsValid)
            return BadRequest(new ValidationProblemDetails(ModelState));

        return Ok(result);
    }
}
