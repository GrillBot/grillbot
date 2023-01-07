using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/search")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class SearchingController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public SearchingController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets paginated list o searches.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost("list")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PaginatedResponse<SearchingListItem>>> GetSearchListAsync([FromBody] GetSearchingListParams parameters)
    {
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Searching.GetSearchingList>();
        var result = await action.ProcessAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Remove searches.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpDelete]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> RemoveSearchesAsync([FromQuery(Name = "id")] long[] ids)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Searching.RemoveSearches>();
        await action.ProcessAsync(ids);
        return Ok();
    }
}
