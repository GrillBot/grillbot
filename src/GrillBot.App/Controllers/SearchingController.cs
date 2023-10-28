using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.Searching;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Searching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/search")]
[ApiExplorerSettings(GroupName = "v1")]
public class SearchingController : Infrastructure.ControllerBase
{
    public SearchingController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Gets paginated list o searches.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost("list")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, User")]
    [ProducesResponseType(typeof(PaginatedResponse<SearchingListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSearchListAsync([FromBody] GetSearchingListParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<GetSearchingList>(parameters);
    }

    /// <summary>
    /// Remove searches.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpDelete]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveSearchesAsync([FromQuery(Name = "id")] long[] ids)
    {
        var idsLogData = new DictionaryObject<string, long>();
        idsLogData.FromCollection(ids.Select((id, index) => new KeyValuePair<string, long>($"[{index}]", id)));
        ApiAction.Init(this, idsLogData);

        return await ProcessAsync<RemoveSearches>(ids);
    }
}
