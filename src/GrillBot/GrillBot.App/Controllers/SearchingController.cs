using GrillBot.App.Services;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/search")]
    [OpenApiTag("Searching", Description = "Searching for team, service, ...")]
    public class SearchingController : Controller
    {
        private SearchingService Service { get; }
        private GrillBotContext DbContext { get; }

        public SearchingController(SearchingService searchingService, GrillBotContext dbContext)
        {
            Service = searchingService;
            DbContext = dbContext;
        }

        /// <summary>
        /// Gets paginated list o searches.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet]
        [OpenApiOperation(nameof(SearchingController) + "_" + nameof(GetSearchListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<PaginatedResponse<SearchingListItem>>> GetSearchListAsync([FromQuery] GetSearchingListParams parameters)
        {
            var data = await Service.GetPaginatedListAsync(parameters);
            return Ok(data);
        }

        /// <summary>
        /// Remove searches.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpDelete]
        [OpenApiOperation(nameof(SearchingController) + "_" + nameof(GetSearchListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> RemoveSearchesAsync([FromQuery(Name = "id")] long[] ids)
        {
            var searches = await DbContext.SearchItems.AsQueryable()
                .Where(o => ids.Contains(o.Id))
                .ToListAsync();

            DbContext.RemoveRange(searches);
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
