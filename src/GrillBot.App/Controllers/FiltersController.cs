using GrillBot.App.Actions.Api.V3.Filters;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Data.Models.API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[JwtAuthorize("Filters")]
[ApiExplorerSettings(GroupName = "v3")]
public class FiltersController : Core.Infrastructure.Actions.ControllerBase
{
    public FiltersController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [HttpPost]
    [ProducesResponseType(typeof(StoredFilterInfo), StatusCodes.Status200OK)]
    public Task<IActionResult> StoreFilterAsync([FromBody] string filterData)
        => ProcessAsync<StoreFilterAction>(filterData);

    [HttpGet("{filterId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetFilterAsync(Guid filterId)
        => ProcessAsync<GetStoredFilterAction>(filterId);
}
