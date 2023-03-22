using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.Searching;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Searching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

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
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PaginatedResponse<SearchingListItem>>> GetSearchListAsync([FromBody] GetSearchingListParams parameters)
    {
        ApiAction.Init(this, parameters);
        return Ok(await ProcessActionAsync<GetSearchingList, PaginatedResponse<SearchingListItem>>(action => action.ProcessAsync(parameters)));
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
        var idsLogData = new DictionaryObject<string, long>();
        idsLogData.FromCollection(ids.Select((id, index) => new KeyValuePair<string, long>($"[{index}]", id)));
        ApiAction.Init(this, idsLogData);

        await ProcessActionAsync<RemoveSearches>(action => action.ProcessAsync(ids));
        return Ok();
    }
}
