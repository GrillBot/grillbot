using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Actions.Api.V3.Auth;

public class OAuth2Action : ApiAction
{
    private readonly IConfiguration _configuration;

    public OAuth2Action(ApiRequestContext context, IConfiguration configuration) : base(context)
    {
        _configuration = configuration;
    }

    public override Task<ApiResult> ProcessAsync()
    {
        var redirect = new RedirectResult(_configuration["Auth:OAuth2:WebRedirectUrl"]!, true);
        return Task.FromResult(new ApiResult(StatusCodes.Status308PermanentRedirect, redirect));
    }
}
