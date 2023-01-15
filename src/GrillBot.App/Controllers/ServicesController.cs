using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.Services;
using GrillBot.Data.Models.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/services")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class ServicesController : Infrastructure.ControllerBase
{
    public ServicesController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get diagnostics information about graphics microservice.
    /// </summary>
    [HttpGet("graphics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<GraphicsServiceInfo>> GetGraphicsServiceInfoAsync()
    {
        var result = await ProcessActionAsync<GetGraphicsServiceInfo, GraphicsServiceInfo>(action => action.ProcessAsync());
        return Ok(result);
    }
}
