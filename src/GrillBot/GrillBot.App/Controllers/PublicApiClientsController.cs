using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.PublicApiClients;
using GrillBot.Database.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/publicApiClients")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class PublicApiClientsController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public PublicApiClientsController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Get list of clients.
    /// </summary>
    /// <response code="200">Returns list of clients</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ApiClient>>> GetClientsListAsync()
    {
        var action = ServiceProvider.GetRequiredService<GetClientsList>();
        var result = await action.ProcessAsync();

        return Ok(result);
    }

    /// <summary>
    /// Create new client.
    /// </summary>
    /// <response code="200"></response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> CreateClientAsync([FromBody] List<string> allowedMethods)
    {
        var action = ServiceProvider.GetRequiredService<CreateClient>();
        await action.ProcessAsync(allowedMethods);

        return Ok();
    }

    /// <summary>
    /// Delete an existing client.
    /// </summary>
    /// <response code="200"></response>
    /// <response code="404">Client wasn't found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteClientAsync(string id)
    {
        var action = ServiceProvider.GetRequiredService<DeleteClient>();
        await action.ProcessAsync(id);

        return Ok();
    }

    /// <summary>
    /// Update existing client.
    /// </summary>
    /// <response code="200"></response>
    /// <response code="404">Client wasn't found.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateClientAsync(string id, List<string> allowedMethods)
    {
        var action = ServiceProvider.GetRequiredService<UpdateClient>();
        await action.ProcessAsync(id, allowedMethods);

        return Ok();
    }
}
