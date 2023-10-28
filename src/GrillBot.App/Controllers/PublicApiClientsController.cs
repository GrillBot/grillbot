using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.PublicApiClients;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.ApiClients;
using GrillBot.Database.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/publicApiClients")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class PublicApiClientsController : Core.Infrastructure.Actions.ControllerBase
{
    public PublicApiClientsController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get list of clients.
    /// </summary>
    /// <response code="200">Returns list of clients</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ApiClient>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientsListAsync()
        => await ProcessAsync<GetClientsList>();

    /// <summary>
    /// Create new client.
    /// </summary>
    /// <response code="200"></response>
    /// <response code="400">Validation failed</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClientAsync([FromBody] ApiClientParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<CreateClient>(parameters);
    }

    /// <summary>
    /// Delete an existing client.
    /// </summary>
    /// <response code="200"></response>
    /// <response code="404">Client wasn't found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteClientAsync(string id)
        => await ProcessAsync<DeleteClient>(id);

    /// <summary>
    /// Update existing client.
    /// </summary>
    /// <response code="200"></response>
    /// <response code="400">Validation failed</response>
    /// <response code="404">Client wasn't found.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClientAsync(string id, [FromBody] ApiClientParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<UpdateClient>(id, parameters);
    }

    /// <summary>
    /// Get client.
    /// </summary>
    /// <response code="200">Returns client data</response>
    /// <response code="404">Client not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiClient), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientAsync(string id)
        => await ProcessAsync<GetClient>(id);
}
