using Discord;
using Discord.Rest;
using GrillBot.Data.Models.API.OAuth2;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable S1075 // URIs should not be hardcoded
namespace GrillBot.App.Services
{
    public class OAuth2Service
    {
        private IConfiguration Configuration { get; }
        private GrillBotContextFactory DbFactory { get; }

        public OAuth2Service(IConfiguration configuration, GrillBotContextFactory dbFactory)
        {
            Configuration = configuration.GetSection("OAuth2");
            DbFactory = dbFactory;
        }

        public OAuth2GetLink GetRedirectLink()
        {
            var builder = new UriBuilder("https://discord.com/api/oauth2/authorize")
            {
                Query = string.Join("&", new[]
                {
                    $"client_id={Configuration["ClientId"]}",
                    $"redirect_uri={WebUtility.UrlEncode(Configuration["RedirectUrl"])}",
                    "response_type=code",
                    "scope=identify"
                })
            };

            return new OAuth2GetLink(builder.ToString());
        }

        public async Task<string> CreateRedirectUrlAsync(string code)
        {
            var accessToken = await CreateAccessTokenAsync(code);

            var uriBuilder = new UriBuilder(Configuration["ClientRedirectUrl"])
            {
                Query = $"sessionId={accessToken}"
            };

            return uriBuilder.ToString();
        }

        private async Task<string> CreateAccessTokenAsync(string code)
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/oauth2/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    { "client_id", Configuration["ClientId"] },
                    { "client_secret", Configuration["ClientSecret"] },
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "scope", "identify" },
                    { "redirect_uri", Configuration["RedirectUrl"] }
                })
            };

            using var httpClient = new HttpClient();
            using var response = await httpClient.SendAsync(message);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new WebException(json);

            return JObject.Parse(json)["access_token"].ToString();
        }

        private static async Task<ulong> GetUserIdAsync(string token)
        {
            using var client = new DiscordRestClient();
            await client.LoginAsync(TokenType.Bearer, token);

            return client.CurrentUser.Id;
        }

        public async Task<OAuth2LoginToken> CreateTokenAsync(string sessionId)
        {
            var userId = await GetUserIdAsync(sessionId);

            using var context = DbFactory.Create();
            var user = await context.Users.FirstOrDefaultAsync(o => o.Id == userId.ToString() && (o.Flags & (int)UserFlags.WebAdmin) != 0);

            if (user == null)
                return new OAuth2LoginToken("Uživatel použitý pod tímto přihlášení nebyl nalezen, nebo nemá oprávnění pro přístup do administrace.");

            var jwt = CreateJwtAccessToken(user, out var expiresAt);
            return new OAuth2LoginToken(jwt, expiresAt);
        }

        private string CreateJwtAccessToken(User user, out DateTimeOffset expiresAt)
        {
            expiresAt = DateTimeOffset.UtcNow.AddHours(3); // Token will be valid for 3 hours.

            var machineInfo = $"{Environment.MachineName}/{Environment.UserName}";
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Audience = $"GrillBot/Audience/{machineInfo}",
                Expires = expiresAt.DateTime,
                IssuedAt = DateTime.UtcNow,
                Issuer = $"GrillBot/Issuer/{machineInfo}",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes($"{Configuration["ClientId"]}_{Configuration["ClientSecret"]}")),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                })
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
