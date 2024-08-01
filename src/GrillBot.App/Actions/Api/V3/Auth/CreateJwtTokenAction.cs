using GrillBot.App.Managers.Auth;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.OAuth2;

namespace GrillBot.App.Actions.Api.V3.Auth;

public class CreateJwtTokenAction : ApiAction
{
    private readonly ITextsManager _texts;
    private readonly JwtTokenManager _jwtTokenManager;

    private OAuth2LoginToken UserNotFound => new(_texts["Auth/CreateToken/UserNotFound", ApiContext.Language]);

    public CreateJwtTokenAction(ApiRequestContext apiContext, ITextsManager texts, JwtTokenManager jwtTokenManager) : base(apiContext)
    {
        _texts = texts;
        _jwtTokenManager = jwtTokenManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var token = ApiContext.LoggedUser is null
            ? UserNotFound
            : await _jwtTokenManager.CreateTokenForUserAsync(ApiContext.LoggedUser, ApiContext.Language);

        return ApiResult.Ok(token);
    }
}
