using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using GrillBot.App.Managers.Auth;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.OAuth2;
using GrillBot.Database.Enums;
using Microsoft.IdentityModel.Tokens;

namespace GrillBot.App.Actions.Api.V1.Auth;

public class CreateToken : ApiAction
{
    private HttpClient HttpClient { get; }
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IConfiguration Configuration { get; }

    public CreateToken(ApiRequestContext apiContext, IHttpClientFactory httpClientFactory, IDiscordClient discordClient, ITextsManager texts, GrillBotDatabaseBuilder databaseBuilder,
        IConfiguration configuration) : base(apiContext)
    {
        HttpClient = httpClientFactory.CreateClient();
        DiscordClient = discordClient;
        Texts = texts;
        DatabaseBuilder = databaseBuilder;
        Configuration = configuration.GetRequiredSection("Auth:OAuth2");
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var isPublic = (bool)Parameters[1]!;

        if (Parameters[0] is string sessionId)
        {
            var userId = await GetUserIdAsync(sessionId);
            var token = await ProcessAsync(userId, isPublic);
            return ApiResult.Ok(token);
        }
        else if (Parameters[0] is ulong userId)
        {
            var token = await ProcessAsync(userId, isPublic);
            return ApiResult.Ok(token);
        }

        return ApiResult.BadRequest();
    }

    private async Task<OAuth2LoginToken> ProcessAsync(ulong? userId, bool isPublic)
    {
        var user = await FindUserAsync(userId);

        if (user is null)
            return new OAuth2LoginToken(Texts["Auth/CreateToken/UserNotFound", ApiContext.Language]);

        using var repository = DatabaseBuilder.CreateRepository();

        var userEntity = await repository.User.FindUserAsync(user, true);
        if (userEntity is null)
            return new OAuth2LoginToken(Texts["Auth/CreateToken/UserNotFound", ApiContext.Language]);

        var checkResult = CheckUserLogin(userEntity, isPublic);
        if (!string.IsNullOrEmpty(checkResult))
            return new OAuth2LoginToken(checkResult);

        var (token, expiresAt) = CreateJwtToken(userEntity, isPublic);
        return new OAuth2LoginToken(token, expiresAt);
    }

    private async Task<ulong?> GetUserIdAsync(string sessionId)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sessionId);

        using var response = await HttpClient.SendAsync(message);
        var json = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return JObject.Parse(json)["id"]!.Value<ulong>();
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return null;

        throw new WebException(json);
    }

    private async Task<IUser?> FindUserAsync(ulong? userId)
        => userId == null ? null : await DiscordClient.FindUserAsync(userId.Value);

    private string? CheckUserLogin(Database.Entity.User user, bool isPublic)
    {
        return isPublic switch
        {
            true when user.HaveFlags(UserFlags.PublicAdministrationBlocked) => Texts["Auth/CreateToken/PublicAdminBlocked", ApiContext.Language].FormatWith(user.Username),
            false when !user.HaveFlags(UserFlags.WebAdmin) => Texts["Auth/CreateToken/PrivateAdminDisabled", ApiContext.Language].FormatWith(user.Username),
            _ => null
        };
    }

    private (string token, DateTimeOffset expiresAt) CreateJwtToken(Database.Entity.User user, bool isPublic)
    {
        var expiresAt = DateTimeOffset.Now.AddHours(3); // Token will be valid for 3 hours.

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
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, isPublic ? "User" : "Admin"),
                new Claim(JwtTokenManager.IP_CLAIM_TYPE, ApiContext.RemoteIp)
            })
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiresAt);
    }
}
