using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.ScheduledJobs;
using GrillBot.Data.Models.API.Jobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/jobs")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class ScheduledJobsController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public ScheduledJobsController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Get nonpaginated list of scheduled jobs.
    /// </summary>
    /// <response code="200">Returns list of scheduled jobs</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ScheduledJob>>> GetScheduledJobsAsync()
    {
        var action = ServiceProvider.GetRequiredService<GetScheduledJobs>();
        var result = await action.ProcessAsync();

        return Ok(result);
    }

    /// <summary>
    /// Trigger scheduled job.
    /// </summary>
    /// <response code="200"></response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RunScheduledJobAsync(string jobName)
    {
        var action = ServiceProvider.GetRequiredService<RunScheduledJob>();
        await action.ProcessAsync(jobName);
        
        return Ok();
    }
}
