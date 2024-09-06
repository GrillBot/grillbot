using GrillBot.App.Managers.Points;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Models.API.OAuth2;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GrillBot.App.Managers.Auth;

public class JwtTokenManager
{
    private readonly GrillBotDatabaseBuilder _databaseBuilder;
    private readonly ITextsManager _texts;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public JwtTokenManager(GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, IServiceProvider serviceProvider,
        IWebHostEnvironment environment, IConfiguration configuration)
    {
        _databaseBuilder = databaseBuilder;
        _texts = texts;
        _serviceProvider = serviceProvider;
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<OAuth2LoginToken> CreateTokenForUserAsync(IUser user, string language, IInteractionContext? interaction = null)
    {
        await using var repository = _databaseBuilder.CreateRepository();

        var userEntity = await repository.User.FindUserAsync(user, true);
        if (userEntity is null)
            return new(_texts["Auth/CreateToken/UserNotFound", language]);

        var userRole = ResolveRole(userEntity, interaction);
        if (string.IsNullOrEmpty(userRole))
            return new OAuth2LoginToken(_texts["Auth/CreateToken/PublicAdminBlocked", language].FormatWith(user.Username));

        await SynchronizeUserToServicesAsync(user);
        var (jwt, expiresAt) = GenerateJwtToken(userEntity, userRole, interaction);
        return new OAuth2LoginToken(jwt, expiresAt);
    }

    private static string? ResolveRole(Database.Entity.User user, IInteractionContext? interaction)
    {
        if (interaction is not null)
            return user.HaveFlags(UserFlags.CommandsDisabled) ? null : "Command";

        if (user.HaveFlags(UserFlags.WebAdminDisabled))
            return null;
        return user.HaveFlags(UserFlags.WebAdmin) ? "Administrator" : "User";
    }

    private (string jwt, DateTimeOffset expiresAt) GenerateJwtToken(Database.Entity.User user, string userRole, IInteractionContext? interaction)
    {
        var expiresAt = ResolveTokenExpiration(userRole);
        var instanceInfo = $"{Environment.MachineName}/{Environment.UserName}";
        var issuer = $"GrillBot/{instanceInfo}";

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
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, userRole),
                new Claim("GrillBot:Permissions", string.Join(",", CreatePermissions(userRole, interaction)))
            })
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiresAt);
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
            _ => now.AddHours(4)
        };
    }

    private async Task SynchronizeUserToServicesAsync(IUser user)
    {
        await _serviceProvider.GetRequiredService<PointsManager>()
            .PushSynchronizationUsersAsync(new[] { user });
    }

    private static IEnumerable<string> CreatePermissions(string role, IInteractionContext? interaction)
    {
        if (interaction is not null)
        {
            yield return interaction.Interaction.Data?.ToString() ?? "-";
            yield return $"GuildId:{interaction.Guild?.Id}";
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
        }
        else
        {
            yield return "Remind(OnlyMyReminders)";
            yield return "Remind(CancelMyReminders)";
            yield return "UserMeasures(OnlyMyMeasures)";
        }

        yield return "FrontendLog";
        yield return "Lookups";
        yield return "Points(Leaderboard)";
        yield return "Points(UserStatus)";
    }
}
