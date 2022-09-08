using GrillBot.App.Services.Unverify;
using GrillBot.Data.Models.API.Unverify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using GrillBot.Data.Models.API;
using GrillBot.Data.Exceptions;
using AutoMapper;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Common.Extensions;
using GrillBot.Common.Models;
using GrillBot.Database.Models;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/unverify")]
[ApiExplorerSettings(GroupName = "v1")]
public class UnverifyController : Controller
{
    private UnverifyService UnverifyService { get; }
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }
    private UnverifyApiService UnverifyApiService { get; }
    private ApiRequestContext ApiRequestContext { get; }

    public UnverifyController(UnverifyService unverifyService, IDiscordClient discordClient,
        IMapper mapper, UnverifyApiService unverifyApiService, ApiRequestContext apiRequestContext)
    {
        UnverifyService = unverifyService;
        DiscordClient = discordClient;
        Mapper = mapper;
        UnverifyApiService = unverifyApiService;
        ApiRequestContext = apiRequestContext;
    }

    /// <summary>
    /// Gets list of current unverifies in guild.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("current")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<UnverifyUserProfile>>> GetCurrentUnverifiesAsync()
    {
        var userId = ApiRequestContext.IsPublic() ? ApiRequestContext.GetUserId() : (ulong?)null;
        var unverifies = await UnverifyService.GetAllUnverifiesAsync(userId);
        var result = Mapper.Map<List<UnverifyUserProfile>>(unverifies.Select(o => o.Item1));

        foreach (var profile in result)
        {
            var entity = unverifies.Find(o => o.Item1.Destination.Id == profile.User.Id.ToUlong());
            profile.Guild = Mapper.Map<Guild>(entity.Item2);
        }

        return Ok(result);
    }

    /// <summary>
    /// Removes unverify
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="userId">User Id</param>
    /// <response code="200">Success</response>
    /// <response code="404">Unverify or guild not found.</response>
    [HttpDelete("{guildId}/{userId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<MessageResponse>> RemoveUnverifyAsync(ulong guildId, ulong userId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId);

        if (guild == null)
            return NotFound(new MessageResponse("Server na kterém by se mělo nacházet unverify nebyl nalezen."));

        await guild.DownloadUsersAsync();
        var toUser = await guild.GetUserAsync(userId);
        if (toUser == null)
            return NotFound(new MessageResponse("Uživatel, kterému mělo být přiřazeno unverify nebyl nalezen."));

        var fromUser = await guild.GetUserAsync(ApiRequestContext.GetUserId());
        var result = await UnverifyService.RemoveUnverifyAsync(guild, fromUser, toUser, fromWeb: true);
        return Ok(new MessageResponse(result));
    }

    /// <summary>
    /// Updates unverify time.
    /// </summary>
    /// <param name="guildId">Guild Id</param>
    /// <param name="userId">User Id</param>
    /// <param name="endTime">New unverify end.</param>
    [HttpPut("{guildId}/{userId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<MessageResponse>> UpdateUnverifyTimeAsync(ulong guildId, ulong userId, [FromQuery, Required] DateTime endTime)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId);

        if (guild == null)
            return NotFound(new MessageResponse("Server na kterém by se mělo nacházet unverify nebyl nalezen."));

        await guild.DownloadUsersAsync();
        var toUser = await guild.GetUserAsync(userId);
        if (toUser == null)
            return NotFound(new MessageResponse("Uživatel, kterému mělo být přiřazeno unverify nebyl nalezen."));

        var fromUser = await guild.GetUserAsync(ApiRequestContext.GetUserId());
        var result = await UnverifyService.UpdateUnverifyAsync(toUser, guild, endTime, fromUser);
        return Ok(new MessageResponse(result));
    }

    /// <summary>
    /// Gets paginated list of unverify logs.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost("log")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PaginatedResponse<UnverifyLogItem>>> GetUnverifyLogsAsync([FromBody] UnverifyLogParams parameters)
    {
        this.StoreParameters(parameters);

        var result = await UnverifyApiService.GetLogsAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Recover state before specific unverify.
    /// </summary>
    /// <param name="logId">ID of log.</param>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Unverify, guild or users not found.</response>
    [HttpPost("log/{logId:long}/recover")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult> RecoverUnverifyAsync(long logId)
    {
        try
        {
            await UnverifyService.RecoverUnverifyState(logId, ApiRequestContext.GetUserId());
        }
        catch (NotFoundException ex)
        {
            return NotFound(new MessageResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Recover", ex.Message);
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        return Ok();
    }
}
