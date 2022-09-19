#pragma warning disable S1075 // URIs should not be hardcoded
using GrillBot.Data.Models.API.OAuth2;
using GrillBot.Database.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Discord.Net;
using GrillBot.Common.Managers.Logging;

namespace GrillBot.App.Services;

public class OAuth2Service
{
    private IConfiguration Configuration { get; }
    private GrillBotDatabaseBuilder DbFactory { get; }
    private LoggingManager LoggingManager { get; }

    public OAuth2Service(IConfiguration configuration, GrillBotDatabaseBuilder dbFactory, LoggingManager loggingManager)
    {
        Configuration = configuration.GetSection("Auth:OAuth2");
        DbFactory = dbFactory;
        LoggingManager = loggingManager;
    }

    private async Task<IUser> GetUserAsync(string token)
    {
        try
        {
            var config = new DiscordRestConfig
            {
                LogLevel = LogSeverity.Verbose
            };

            await using var client = new DiscordRestClient(config);
            client.Log += LoggingManager.InvokeAsync;
            await client.LoginAsync(TokenType.Bearer, token);

            return client.CurrentUser;
        }
        catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }
    }

    public async Task<OAuth2LoginToken> CreateTokenAsync(string sessionId, bool isPublic)
    {
        var user = await GetUserAsync(sessionId);
        return await CreateTokenAsync(user, isPublic);
    }

    public async Task<OAuth2LoginToken> CreateTokenAsync(IUser user, bool isPublic)
    {
        if (user == null)
            return new OAuth2LoginToken("Proveden pokus s neplatným tokenem. Opakujte přihlášení.");

        await using var repository = DbFactory.CreateRepository();
        var dbUser = await repository.User.FindUserAsync(user, true);

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
