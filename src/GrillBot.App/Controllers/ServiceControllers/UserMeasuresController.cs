using GrillBot.App.Actions.Api.V3.Services.UserMeasures;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.UserMeasures;
using GrillBot.Core.Services.UserMeasures.Models.Dashboard;
using GrillBot.Core.Services.UserMeasures.Models.Measures;
using GrillBot.Data.Models.API.UserMeasures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

public class UserMeasuresController : ServiceControllerBase<IUserMeasuresServiceClient>
{
    public UserMeasuresController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [HttpPost("measures-list")]
    [ProducesResponseType(typeof(PaginatedResponse<MeasuresItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetMeasuresListAsync([FromBody] MeasuresListParams parameters)
        => ExecuteAsync(async client => await client.GetMeasuresListAsync(parameters), parameters);

    [HttpDelete("{measureId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    public Task<IActionResult> DeleteMeasureAsync([FromBody] Guid measureId)
        => ExecuteAsync(async client => await client.DeleteMeasureAsync(DeleteMeasuresRequest.FromInternalId(measureId)));

    [HttpPost("member-warning")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> CreateMemberWarningAsync([FromBody] CreateMemberWarningParams parameters)
        => ExecuteAsync<CreateMemberWarningAction>(parameters);

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(List<DashboardRow>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetDashboard()
        => ExecuteAsync(async client => await client.GetDashboardDataAsync());
}
