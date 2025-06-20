using GrillBot.App.Managers.Points;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Models.API.OAuth2;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GrillBot.App.Managers.Auth;

public class JwtTokenManager(
    GrillBotDatabaseBuilder _databaseBuilder,
    ITextsManager _texts,
    IServiceProvider _serviceProvider,
    IWebHostEnvironment _environment,
    IConfiguration _configuration
)
{
    public const string IP_CLAIM_TYPE = "https://grillbot.eu/claims/ip";

    public async Task<OAuth2LoginToken> CreateTokenForUserAsync(IUser user, string language, string ip, IInteractionContext? interaction = null)
    {
        using var repository = _databaseBuilder.CreateRepository();

        var userEntity = await repository.User.FindUserAsync(user, true);
        if (userEntity is null)
            return new(_texts["Auth/CreateToken/UserNotFound", language]);

        var userRole = ResolveRole(userEntity, interaction);
        if (string.IsNullOrEmpty(userRole))
            return new OAuth2LoginToken(_texts["Auth/CreateToken/PublicAdminBlocked", language].FormatWith(user.Username));

        await SynchronizeUserToServicesAsync(user);
        return GenerateJwtToken(userEntity, userRole, interaction, null, ip);
    }

    public OAuth2LoginToken CreateTokenForApiClient(ApiClient client, string language, string ip)
    {
        var userAvatarId = SnowflakeUtils.ToSnowflake(DateTimeOffset.UtcNow);
        var userEntity = new User
        {
            GlobalAlias = client.Name,
            Language = language,
            Id = client.Id,
            Status = UserStatus.Online,
            Username = client.Name,
            AvatarUrl = CDN.GetDefaultUserAvatarUrl(userAvatarId)
        };

        return GenerateJwtToken(userEntity, "ApiV2", null, client, ip);
    }

    private static string? ResolveRole(User user, IInteractionContext? interaction)
    {
        if (interaction is not null)
            return user.HaveFlags(UserFlags.CommandsDisabled) ? null : "Command";

        if (user.HaveFlags(UserFlags.WebAdminDisabled))
            return null;
        return user.HaveFlags(UserFlags.WebAdmin) ? "Administrator" : "User";
    }

    private OAuth2LoginToken GenerateJwtToken(User user, string userRole, IInteractionContext? interaction, ApiClient? apiClient, string ip)
    {
        var expiresAt = ResolveTokenExpiration(userRole);
        var issuer = $"GrillBot/{Environment.MachineName}/{Environment.UserName}";

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = issuer,
            Expires = expiresAt.UtcDateTime,
            IssuedAt = DateTime.UtcNow,
            Issuer = issuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes($"{_configuration["Auth:OAuth2:ClientId"]}_{_configuration["Auth:OAuth2:ClientSecret"]}")),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Subject = new ClaimsIdentity(ResolveClaims(user, userRole, interaction, apiClient, ip))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new OAuth2LoginToken(tokenHandler.WriteToken(token), expiresAt);
    }

    private DateTimeOffset ResolveTokenExpiration(string userRole)
    {
        var now = DateTimeOffset.Now;

        if (_environment.IsDevelopment())
            return now.AddMonths(1);

        return userRole switch
        {
            "Administrator" => now.AddDays(1),
            "Command" => now.AddMinutes(20),
            "ApiV2" => now.AddMinutes(10),
            "User" => now.AddHours(4),
            _ => throw new NotSupportedException()
        };
    }

    private static IEnumerable<Claim> ResolveClaims(User user, string userRole, IInteractionContext? interaction, ApiClient? apiClient, string ip)
    {
        yield return new Claim(ClaimTypes.Name, user.Username);
        yield return new Claim(ClaimTypes.NameIdentifier, user.Id);
        yield return new Claim(ClaimTypes.Role, userRole);
        yield return new Claim("GrillBot:Permissions", string.Join(",", CreatePermissions(userRole, interaction, apiClient)));
        yield return new Claim(IP_CLAIM_TYPE, ip);

        if (!string.IsNullOrEmpty(user.AvatarUrl))
            yield return new Claim(ClaimTypes.UserData, user.AvatarUrl);

        if (apiClient is not null)
            yield return new Claim("GrillBot:ThirdPartyKey", apiClient.Id);
    }

    private static IEnumerable<string> CreatePermissions(string role, IInteractionContext? interaction, ApiClient? apiClient)
    {
        if (interaction is not null)
        {
            yield return interaction.Interaction.Data?.ToString() ?? "-";
            yield return $"GuildId:{interaction.Guild?.Id}";
            yield break;
        }

        if (apiClient is not null)
        {
            foreach (var method in apiClient.AllowedMethods)
                yield return method;
            yield break;
        }

        if (role == "Administrator")
        {
            yield return "Dashboard(Admin)";
            yield return "AuditLog(Admin)";
            yield return "Emote(Admin)";
            yield return "Points(Admin)";
            yield return "Remind(Admin)";
            yield return "UserMeasures(Admin)";
            yield return "Searching(Admin)";
            yield return "Invite(Admin)";
            yield return "Message(Admin)";
        }
        else
        {
            yield return "Remind(OnlyMyReminders)";
            yield return "Remind(CancelMyReminders)";
            yield return "UserMeasures(OnlyMyMeasures)";
            yield return "Searching(OnlyMySearches)";
        }

        yield return "FrontendLog";
        yield return "Lookups";
        yield return "Points(Leaderboard)";
        yield return "Points(UserStatus)";
        yield return "Filters";
    }

    private async Task SynchronizeUserToServicesAsync(IUser user)
    {
        await _serviceProvider.GetRequiredService<PointsManager>()
            .PushSynchronizationUsersAsync(new[] { user });
    }
}
