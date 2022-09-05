using GrillBot.App.Services;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/search")]
public class SearchingController : Controller
{
    private SearchingService Service { get; }
    private ApiRequestContext ApiRequestContext { get; }

    public SearchingController(SearchingService searchingService, ApiRequestContext apiRequestContext)
    {
        Service = searchingService;
        ApiRequestContext = apiRequestContext;
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
        this.StoreParameters(parameters);

        var data = await Service.GetPaginatedListAsync(parameters, ApiRequestContext);
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
