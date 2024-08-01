using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.SearchingService;
using GrillBot.Core.Services.SearchingService.Models.Request;
using GrillBot.Core.Services.SearchingService.Models.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

public class SearchingController : ServiceControllerBase<ISearchingServiceClient>
{
    public SearchingController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [HttpPost("list")]
    [JwtAuthorize("Searching(Admin)", "Searching(OnlyMySearches)")]
    [ProducesResponseType(typeof(PaginatedResponse<SearchListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetSearchingListAsync(SearchingListRequest request)
        => ExecuteAsync(async client => await client.GetSearchingListAsync(request));

    [HttpDelete("{id}")]
    [JwtAuthorize("Searching(Admin)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> RemoveSearchingAsync(long id)
        => ExecuteAsync(async client => await client.RemoveSearchingAsync(id));
}
