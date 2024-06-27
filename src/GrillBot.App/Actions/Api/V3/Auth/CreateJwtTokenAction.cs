using GrillBot.App.Managers.Points;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.OAuth2;
using GrillBot.Database.Enums;
using GrillBot.Database.Migrations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GrillBot.App.Actions.Api.V3.Auth;

public class CreateJwtTokenAction : ApiAction
{
    private readonly ITextsManager _texts;
    private readonly GrillBotDatabaseBuilder _databaseBuilder;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    private OAuth2LoginToken UserNotFound => new(_texts["Auth/CreateToken/UserNotFound", ApiContext.Language]);

    public CreateJwtTokenAction(ApiRequestContext apiContext, ITextsManager texts, GrillBotDatabaseBuilder databaseBuilder,
        IWebHostEnvironment environment, IConfiguration configuration, IServiceProvider serviceProvider) : base(apiContext)
    {
        _texts = texts;
        _databaseBuilder = databaseBuilder;
        _environment = environment;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var token = await CreateTokenAsync();
        return ApiResult.Ok(token);
    }

    private async Task<OAuth2LoginToken> CreateTokenAsync()
    {
        if (ApiContext.LoggedUser is null)
            return UserNotFound;

        await using var repository = _databaseBuilder.CreateRepository();

        var user = await repository.User.FindUserAsync(ApiContext.LoggedUser, true);
        if (user is null)
            return UserNotFound;

        var userRole = ResolveRole(user);
        if (string.IsNullOrEmpty(userRole))
            return new OAuth2LoginToken(_texts["Auth/CreateToken/PublicAdminBlocked", ApiContext.Language].FormatWith(user.Username));

        await SynchronizeUserToServicesAsync();
        var (jwt, expiresAt) = GenerateJwtToken(user, userRole);
        return new OAuth2LoginToken(jwt, expiresAt);
    }

    private static string? ResolveRole(Database.Entity.User user)
    {
        if (user.HaveFlags(UserFlags.WebAdminDisabled))
            return null;

        return user.HaveFlags(UserFlags.WebAdmin) ? "Administrator" : "User";
    }

    private (string jwt, DateTimeOffset expiresAt) GenerateJwtToken(Database.Entity.User user, string userRole)
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
                new Claim(ClaimTypes.Role, userRole)
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
            _ => now.AddHours(4)
        };
    }

    private async Task SynchronizeUserToServicesAsync()
    {
        await _serviceProvider.GetRequiredService<PointsManager>()
            .PushSynchronizationUsersAsync(new[] { ApiContext.LoggedUser! });
    }
}
