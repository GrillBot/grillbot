using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.Emote.Models.Request;
using GrillBot.Core.Services.Emote.Models.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

[JwtAuthorize("Emote(Admin)")]
public class EmoteController : ServiceControllerBase<IEmoteServiceClient>
{
    public EmoteController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [HttpPost("statistics-list")]
    [ProducesResponseType(typeof(PaginatedResponse<EmoteStatisticsItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetEmoteStatisticsListAsync(EmoteStatisticsListRequest request)
        => ExecuteAsync(async client => await client.GetEmoteStatisticsListAsync(request), request);

    [HttpGet("{guildId}/{emoteId}")]
    [ProducesResponseType(typeof(EmoteInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetEmoteInfoAsync(string guildId, string emoteId)
        => ExecuteAsync(async client => await client.GetEmoteInfoAsync(guildId, emoteId));

    [HttpPut("merge/{guildId}/{sourceEmoteId}/{destinationEmoteId}")]
    [ProducesResponseType(typeof(MergeStatisticsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> MergeStatisticsAsync(string guildId, string sourceEmoteId, string destinationEmoteId)
        => ExecuteAsync(async client => await client.MergeStatisticsAsync(guildId, sourceEmoteId, destinationEmoteId));

    [HttpPost("usage-list")]
    [ProducesResponseType(typeof(PaginatedResponse<EmoteUserUsageItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetUserEmoteUsageListAsync(EmoteUserUsageListRequest request)
        => ExecuteAsync(async client => await client.GetUserEmoteUsageListAsync(request), request);

    [HttpGet("supported-emotes")]
    [ProducesResponseType(typeof(List<EmoteDefinition>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSupportedEmotesListAsync([FromQuery] string? guildId = null)
        => ExecuteAsync(async client => await client.GetSupportedEmotesListAsync(guildId));

    [HttpDelete("supported-emotes/{guildId}/{emoteId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DeleteSupportedEmotesAsync(string guildId, string emoteId)
        => ExecuteAsync(async client => await client.DeleteSupportedEmoteAsync(guildId, emoteId));

    [HttpDelete("{guildId}/{emoteId}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DeleteStatisticsAsync(string guildId, string emoteId, [FromQuery] string? userId = null)
        => ExecuteAsync(async client => await client.DeleteStatisticsAsync(guildId, emoteId, userId));
}
