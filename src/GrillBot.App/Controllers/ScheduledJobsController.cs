using GrillBot.App.Actions.Api.V1.ScheduledJobs;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Jobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/jobs")]
[ApiExplorerSettings(GroupName = "v1")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class ScheduledJobsController : Infrastructure.ControllerBase
{
    public ScheduledJobsController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get nonpaginated list of scheduled jobs.
    /// </summary>
    /// <response code="200">Returns list of scheduled jobs</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ScheduledJob>>> GetScheduledJobsAsync()
        => Ok(await ProcessActionAsync<GetScheduledJobs, List<ScheduledJob>>(action => action.ProcessAsync()));

    /// <summary>
    /// Trigger a scheduled job.
    /// </summary>
    /// <response code="200"></response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RunScheduledJobAsync(string jobName)
    {
        await ProcessActionAsync<RunScheduledJob>(action => action.ProcessAsync(jobName));
        return Ok();
    }

    /// <summary>
    /// Update an existing job.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="404">Job wasn't found.</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateJobAsync(string jobName, bool enabled)
    {
        await ProcessActionAsync<UpdateJob>(action => action.ProcessAsync(jobName, enabled));
        return Ok();
    }
}
