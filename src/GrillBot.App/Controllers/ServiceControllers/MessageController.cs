using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using MessageService;
using MessageService.Models.Request.AutoReply;
using MessageService.Models.Response.AutoReply;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

[JwtAuthorize("Message(Admin)")]
public class MessageController(IServiceProvider serviceProvider) : ServiceControllerBase<IMessageServiceClient>(serviceProvider)
{
    [HttpPost("autoreply")]
    [ProducesResponseType<AutoReplyDefinition>(StatusCodes.Status200OK)]
    public Task<IActionResult> CreateAutoReplyDefinition([FromBody] AutoReplyDefinitionRequest request)
        => ExecuteAsync(async (client, ctx) => await client.CreateAutoReplyDefinition(request, ctx.CancellationToken), request);

    [HttpGet("autoreply/{id:guid}")]
    [ProducesResponseType<AutoReplyDefinition>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetAutoReplyDefinition(Guid id)
        => ExecuteAsync(async (client, ctx) => await client.GetAutoReplyDefinition(id, ctx.CancellationToken));

    [HttpPost("autoreply/list")]
    [ProducesResponseType(typeof(PaginatedResponse<AutoReplyDefinition>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetAutoReplyDefinitionList([FromBody] AutoReplyDefinitionListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetAutoReplyDefinitionListAsync(request, ctx.CancellationToken), request);

    [HttpDelete("autoreply/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DeleteAutoReplyDefinition(Guid id)
        => ExecuteAsync(async (client, ctx) => await client.DeleteAutoReplyDefinitionAsync(id, ctx.CancellationToken));

    [HttpPut("autoreply/{id:guid}")]
    [ProducesResponseType<AutoReplyDefinition>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> UpdateAutoReplyDefinition(Guid id, [FromBody] AutoReplyDefinitionRequest request)
        => ExecuteAsync(async (client, ctx) => await client.UpdateAutoReplyDefinitionAsync(id, request, ctx.CancellationToken), request);
}
