using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.Emote.Models.Events.Suggestions;
using GrillBot.Core.Services.Emote.Models.Request;
using GrillBot.Core.Services.Emote.Models.Request.EmoteSuggestions;
using GrillBot.Core.Services.Emote.Models.Request.Guild;
using GrillBot.Core.Services.Emote.Models.Response;
using GrillBot.Core.Services.Emote.Models.Response.EmoteSuggestions;
using GrillBot.Core.Services.Emote.Models.Response.Guild;
using GrillBot.Core.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

[JwtAuthorize("Emote(Admin)")]
public class EmoteController(IServiceProvider serviceProvider) : ServiceControllerBase<IEmoteServiceClient>(serviceProvider)
{
    [HttpPost("statistics-list")]
    [ProducesResponseType<PaginatedResponse<EmoteStatisticsItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetEmoteStatisticsListAsync(EmoteStatisticsListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetEmoteStatisticsListAsync(request, ctx.CancellationToken), request);

    [HttpGet("{guildId}/{emoteId}")]
    [ProducesResponseType<EmoteInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetEmoteInfoAsync(string guildId, string emoteId)
        => ExecuteAsync(async (client, ctx) => await client.GetEmoteInfoAsync(guildId, emoteId, ctx.CancellationToken));

    [HttpPut("merge/{guildId}/{sourceEmoteId}/{destinationEmoteId}")]
    [ProducesResponseType<MergeStatisticsResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> MergeStatisticsAsync(string guildId, string sourceEmoteId, string destinationEmoteId)
        => ExecuteAsync(async (client, ctx) => await client.MergeStatisticsAsync(guildId, sourceEmoteId, destinationEmoteId, ctx.AuthorizationToken, ctx.CancellationToken));

    [HttpPost("usage-list")]
    [ProducesResponseType<PaginatedResponse<EmoteUserUsageItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetUserEmoteUsageListAsync(EmoteUserUsageListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetUserEmoteUsageListAsync(request, ctx.CancellationToken), request);

    [HttpGet("supported-emotes")]
    [ProducesResponseType<List<EmoteDefinition>>(StatusCodes.Status200OK)]
    public Task<IActionResult> GetSupportedEmotesListAsync([FromQuery] string? guildId = null)
        => ExecuteAsync(async (client, ctx) => await client.GetSupportedEmotesListAsync(guildId, ctx.CancellationToken));

    [HttpDelete("supported-emotes/{guildId}/{emoteId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DeleteSupportedEmotesAsync(string guildId, string emoteId)
        => ExecuteAsync(async (client, ctx) => await client.DeleteSupportedEmoteAsync(guildId, emoteId, ctx.CancellationToken));

    [HttpDelete("{guildId}/{emoteId}")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DeleteStatisticsAsync(string guildId, string emoteId, [FromQuery] string? userId = null)
        => ExecuteAsync(async (client, ctx) => await client.DeleteStatisticsAsync(guildId, emoteId, userId, ctx.AuthorizationToken, ctx.CancellationToken));

    [HttpPost("emote-suggestions/list")]
    [ProducesResponseType<PaginatedResponse<EmoteSuggestionItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetEmoteSuggestionsListAsync(EmoteSuggestionsListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetEmoteSuggestionsAsync(request, ctx.CancellationToken), request);

    [HttpPut("emote-suggestions/approve/{suggestionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> SetSuggestionApprovalAsync(Guid suggestionId, [FromQuery] bool isApproved)
        => ExecuteAsync(async (client, ctx) => await client.SetSuggestionApprovalAsync(suggestionId, isApproved, ctx.AuthorizationToken, ctx.CancellationToken));

    [HttpPost("emote-suggestions/{suggestionId:guid}/votes")]
    [ProducesResponseType<PaginatedResponse<EmoteSuggestionVoteItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetEmoteSuggestionVotesAsync(Guid suggestionId, EmoteSuggestionVoteListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetSuggestionVotesAsync(suggestionId, request, ctx.CancellationToken), request);

    [HttpDelete("emote-suggestions/{suggestionId:guid}/votes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IActionResult> EmoteSuggestionsCancelVoteAsync(Guid suggestionId)
        => ExecuteRabbitPayloadAsync(new EmoteSuggestionCancelVotePayload(suggestionId));

    [HttpPut("guilds/{guildId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> EmoteUpdateGuildAsync([DiscordId] ulong guildId, GuildRequest request)
        => ExecuteAsync(async (client, ctx) => await client.UpdateGuildAsync(guildId, request, ctx.CancellationToken), request);

    [HttpGet("guilds/{guildId}")]
    [ProducesResponseType<GuildData>(StatusCodes.Status200OK)]
    public Task<IActionResult> EmoteGetGuildAsync([DiscordId] ulong guildId)
        => ExecuteAsync(async (client, ctx) => await client.GetGuildAsync(guildId, ctx.CancellationToken));
}
