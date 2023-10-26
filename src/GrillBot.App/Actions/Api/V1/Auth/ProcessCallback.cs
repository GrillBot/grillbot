using System.Net.Http;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Actions.Api.V1.Auth;

public class ProcessCallback : ApiAction
{
    private IConfiguration Configuration { get; }
    private HttpClient HttpClient { get; }

    public ProcessCallback(ApiRequestContext apiContext, IConfiguration configuration, IHttpClientFactory httpClientFactory) : base(apiContext)
    {
        Configuration = configuration.GetRequiredSection("Auth:OAuth2");
        HttpClient = httpClientFactory.CreateClient();
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var code = (string)Parameters[0]!;
        var encodedState = (string)Parameters[1]!;

        var state = AuthState.Decode(encodedState);
        var accessToken = await RetrieveAccessTokenAsync(code);
        var returnUrl = GetReturnUrl(state);
        var uriBuilder = new UriBuilder(returnUrl);

        if (!string.IsNullOrEmpty(accessToken))
            uriBuilder.Query = string.Join("&", $"sessionId={accessToken}", $"isPublic={state.IsPublic}");

        return new ApiResult(StatusCodes.Status302Found, new RedirectResult(uriBuilder.ToString()));
    }

    private async Task<string?> RetrieveAccessTokenAsync(string code)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/oauth2/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", Configuration["ClientId"]! },
                { "client_secret", Configuration["ClientSecret"]! },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "scope", "identify" },
                { "redirect_uri", Configuration["RedirectUrl"]! }
            })
        };

        using var response = await HttpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return JObject.Parse(content)["access_token"]!.ToString();

        try
        {
            var jsonData = JObject.Parse(content);
            if (jsonData.ContainsKey("error") && jsonData["error"]!.ToString() == "invalid_grant")
                return null;
            throw new WebException(content);
        }
        catch (Exception ex) when (ex is not WebException)
        {
            throw new WebException(content);
        }
    }

    private string GetReturnUrl(AuthState state)
    {
        if (!string.IsNullOrEmpty(state.ReturnUrl))
            return state.ReturnUrl;
        else
            return Configuration[state.IsPublic ? "ClientRedirectUrl" : "AdminRedirectUrl"]!;
    }
}
