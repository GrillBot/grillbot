using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.Emote.Models.Request;
using GrillBot.Core.Services.Emote.Models.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

[JwtAuthorize("Emote(Admin)")]
public class EmoteController(IServiceProvider serviceProvider) : ServiceControllerBase<IEmoteServiceClient>(serviceProvider)
{
    [HttpPost("statistics-list")]
    [ProducesResponseType(typeof(PaginatedResponse<EmoteStatisticsItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetEmoteStatisticsListAsync(EmoteStatisticsListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetEmoteStatisticsListAsync(request, ctx.CancellationToken), request);

    [HttpGet("{guildId}/{emoteId}")]
    [ProducesResponseType(typeof(EmoteInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetEmoteInfoAsync(string guildId, string emoteId)
        => ExecuteAsync(async (client, ctx) => await client.GetEmoteInfoAsync(guildId, emoteId, ctx.CancellationToken));

    [HttpPut("merge/{guildId}/{sourceEmoteId}/{destinationEmoteId}")]
    [ProducesResponseType(typeof(MergeStatisticsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> MergeStatisticsAsync(string guildId, string sourceEmoteId, string destinationEmoteId)
        => ExecuteAsync(async (client, ctx) => await client.MergeStatisticsAsync(guildId, sourceEmoteId, destinationEmoteId, ctx.AuthorizationToken, ctx.CancellationToken));

    [HttpPost("usage-list")]
    [ProducesResponseType(typeof(PaginatedResponse<EmoteUserUsageItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetUserEmoteUsageListAsync(EmoteUserUsageListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetUserEmoteUsageListAsync(request, ctx.CancellationToken), request);

    [HttpGet("supported-emotes")]
    [ProducesResponseType(typeof(List<EmoteDefinition>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSupportedEmotesListAsync([FromQuery] string? guildId = null)
        => ExecuteAsync(async (client, ctx) => await client.GetSupportedEmotesListAsync(guildId, ctx.CancellationToken));

    [HttpDelete("supported-emotes/{guildId}/{emoteId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DeleteSupportedEmotesAsync(string guildId, string emoteId)
        => ExecuteAsync(async (client, ctx) => await client.DeleteSupportedEmoteAsync(guildId, emoteId, ctx.CancellationToken));

    [HttpDelete("{guildId}/{emoteId}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DeleteStatisticsAsync(string guildId, string emoteId, [FromQuery] string? userId = null)
        => ExecuteAsync(async (client, ctx) => await client.DeleteStatisticsAsync(guildId, emoteId, userId, ctx.AuthorizationToken, ctx.CancellationToken));
}
