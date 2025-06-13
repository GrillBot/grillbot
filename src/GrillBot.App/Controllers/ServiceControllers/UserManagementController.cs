using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Services.UserManagementService;
using GrillBot.Core.Services.UserManagementService.Models.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

[JwtAuthorize("User(Admin)")]
public class UserManagementController(IServiceProvider serviceProvider) : ServiceControllerBase<IUserManagementServiceClient>(serviceProvider)
{
    [HttpGet("user/{userId}")]
    [ProducesResponseType<UserInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetUserInfoAsync(ulong userId)
        => ExecuteAsync(async (client, ctx) => await client.GetUserInfoAsync(userId, ctx.CancellationToken));
}
