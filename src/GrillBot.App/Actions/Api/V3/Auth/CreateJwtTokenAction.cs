using GrillBot.App.Managers.Auth;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.OAuth2;

namespace GrillBot.App.Actions.Api.V3.Auth;

public class CreateJwtTokenAction(
    ApiRequestContext _apiContext,
    ITextsManager _texts,
    JwtTokenManager _jwtTokenManager
) : ApiAction(_apiContext)
{
    private OAuth2LoginToken UserNotFound => new(_texts["Auth/CreateToken/UserNotFound", ApiContext.Language]);

    public override async Task<ApiResult> ProcessAsync()
    {
        var token = ApiContext.LoggedUser is null
            ? UserNotFound
            : await _jwtTokenManager.CreateTokenForUserAsync(ApiContext.LoggedUser, ApiContext.Language, ApiContext.RemoteIp);

        return ApiResult.Ok(token);
    }
}
