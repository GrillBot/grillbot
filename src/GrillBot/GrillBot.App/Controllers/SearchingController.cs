using GrillBot.App.Services;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/search")]
[OpenApiTag("Searching", Description = "Searching for team, service, ...")]
public class SearchingController : Controller
{
    private SearchingService Service { get; }

    public SearchingController(SearchingService searchingService)
    {
        Service = searchingService;
    }

    /// <summary>
    /// Gets paginated list o searches.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PaginatedResponse<SearchingListItem>>> GetSearchListAsync([FromQuery] GetSearchingListParams parameters,
        CancellationToken cancellationToken)
    {
        var data = await Service.GetPaginatedListAsync(parameters, User, cancellationToken);
        return Ok(data);
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
        await Service.RemoveSearchesAsync(ids);
        return Ok();
    }
}
