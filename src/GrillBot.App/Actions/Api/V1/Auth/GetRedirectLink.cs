using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.OAuth2;

namespace GrillBot.App.Actions.Api.V1.Auth;

public class GetRedirectLink : ApiAction
{
    private IConfiguration Configuration { get; }

    public GetRedirectLink(ApiRequestContext apiContext, IConfiguration configuration) : base(apiContext)
    {
        Configuration = configuration.GetRequiredSection("Auth:OAuth2");
    }

    public override Task<ApiResult> ProcessAsync()
    {
        var state = (AuthState)Parameters[0]!;
        var builder = new UriBuilder("https://discord.com/api/oauth2/authorize")
        {
            Query = string.Join(
                "&",
                $"client_id={Configuration["ClientId"]}",
                $"redirect_uri={WebUtility.UrlEncode(Configuration["RedirectUrl"])}",
                "response_type=code",
                "scope=identify",
                $"state={state.Encode()}"
            )
        };

        return Task.FromResult(ApiResult.Ok(new OAuth2GetLink(builder.ToString())));
    }
}
