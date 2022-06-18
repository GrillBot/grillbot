#pragma warning disable S1075 // URIs should not be hardcoded
using GrillBot.App.Services.Logging;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.OAuth2;
using GrillBot.Database.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;

namespace GrillBot.App.Services;

public class OAuth2Service
{
    private IConfiguration Configuration { get; }
    private GrillBotDatabaseBuilder DbFactory { get; }
    private LoggingService LoggingService { get; }
    private HttpClient HttpClient { get; }

    public OAuth2Service(IConfiguration configuration, GrillBotDatabaseBuilder dbFactory, LoggingService loggingService,
        IHttpClientFactory httpClientFactory)
    {
        Configuration = configuration.GetSection("Auth:OAuth2");
        DbFactory = dbFactory;
        LoggingService = loggingService;
        HttpClient = httpClientFactory.CreateClient();
    }

    public OAuth2GetLink GetRedirectLink(AuthState state)
    {
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

        return new OAuth2GetLink(builder.ToString());
    }

    public async Task<string> CreateRedirectUrlAsync(string code, string encodedState, CancellationToken cancellationToken)
    {
        var state = AuthState.Decode(encodedState);
        var accessToken = await CreateAccessTokenAsync(code, cancellationToken);
        var redirectUrl = GetReturnUrl(state);

        var uriBuilder = new UriBuilder(redirectUrl)
        {
            Query = string.Join("&", $"sessionId={accessToken}", $"isPublic={state.IsPublic}")
        };

        return uriBuilder.ToString();
    }

    private string GetReturnUrl(AuthState state)
        => !string.IsNullOrEmpty(state.ReturnUrl) ? state.ReturnUrl : Configuration[state.IsPublic ? "ClientRedirectUrl" : "AdminRedirectUrl"];

    private async Task<string> CreateAccessTokenAsync(string code, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/oauth2/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", Configuration["ClientId"] },
                { "client_secret", Configuration["ClientSecret"] },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "scope", "identify" },
                { "redirect_uri", Configuration["RedirectUrl"] }
            })
        };

        using var response = await HttpClient.SendAsync(message, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new WebException(json);

        return JObject.Parse(json)["access_token"]!.ToString();
    }

    private async Task<IUser> GetUserAsync(string token)
    {
        var config = new DiscordRestConfig
        {
            LogLevel = LogSeverity.Verbose
        };

        await using var client = new DiscordRestClient(config);
        client.Log += LoggingService.OnLogAsync;
        await client.LoginAsync(TokenType.Bearer, token);

        return client.CurrentUser;
    }

    public async Task<OAuth2LoginToken> CreateTokenAsync(string sessionId, bool isPublic, CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(sessionId);

        await using var repository = DbFactory.CreateRepository();
        var dbUser = await repository.User.FindUserAsync(user);

        if (dbUser == null)
            return new OAuth2LoginToken($"Uživatel {user.Username} nebyl nalezen.");

        if (isPublic)
        {
            if (dbUser.HaveFlags(UserFlags.PublicAdministrationBlocked))
                return new OAuth2LoginToken($"Uživatel {user.Username} má zablokovaný přístup do osobní administrace.");
        }
        else
        {
            if (!dbUser.HaveFlags(UserFlags.WebAdmin))
                return new OAuth2LoginToken($"Uživatel {user.Username} nemá oprávnění pro přístup do administrace.");
        }

        var jwt = CreateJwtAccessToken(dbUser, isPublic, out var expiresAt);
        return new OAuth2LoginToken(jwt, expiresAt);
    }

    private string CreateJwtAccessToken(Database.Entity.User user, bool isPublic, out DateTimeOffset expiresAt)
    {
        expiresAt = DateTimeOffset.UtcNow.AddHours(3); // Token will be valid for 3 hours.

        var machineInfo = $"{Environment.MachineName}/{Environment.UserName}";
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = $"GrillBot/{machineInfo}",
            Expires = expiresAt.DateTime,
            IssuedAt = DateTime.UtcNow,
            Issuer = $"GrillBot/{machineInfo}",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes($"{Configuration["ClientId"]}_{Configuration["ClientSecret"]}")),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, $"{user.Username}#{user.Discriminator}"),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, isPublic ? "User" : "Admin")
            })
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
