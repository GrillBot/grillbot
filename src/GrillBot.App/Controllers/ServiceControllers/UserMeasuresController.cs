using GrillBot.App.Actions.Api.V3.Services.UserMeasures;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using UserMeasures;
using UserMeasures.Models.Dashboard;
using UserMeasures.Models.Measures;
using GrillBot.Data.Models.API.UserMeasures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

[JwtAuthorize("UserMeasures(Admin)")]
public class UserMeasuresController(IServiceProvider serviceProvider) : ServiceControllerBase<IUserMeasuresServiceClient>(serviceProvider)
{
    [HttpPost("measures-list")]
    [ProducesResponseType(typeof(PaginatedResponse<MeasuresItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetMeasuresListAsync([FromBody] MeasuresListParams parameters)
        => ExecuteAsync(async (client, ctx) => await client.GetMeasuresListAsync(parameters, ctx.CancellationToken), parameters);

    [HttpDelete("{measureId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    public Task<IActionResult> DeleteMeasureAsync([FromRoute] Guid measureId)
        => ExecuteAsync(async (client, ctx) => await client.DeleteMeasureAsync(DeleteMeasuresRequest.FromInternalId(measureId), ctx.CancellationToken));

    [HttpPost("member-warning")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> CreateMemberWarningAsync([FromBody] CreateMemberWarningParams parameters)
        => ExecuteAsync<CreateMemberWarningAction>(parameters);

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(List<DashboardRow>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetDashboard()
        => ExecuteAsync(async (client, ctx) => await client.GetDashboardDataAsync(ctx.CancellationToken));
}
