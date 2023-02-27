using System.Reflection;
using System.Security.Claims;
using GrillBot.App.Actions;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.Infrastructure.Common;

public abstract class ApiActionTest<TAction> : TestBase<TAction> where TAction : ApiAction
{
    private ApiConfigurationAttribute? ApiConfiguration
        => GetMethod().GetCustomAttribute<ApiConfigurationAttribute>();

    protected bool IsPublic => ApiConfiguration?.IsPublic ?? false;

    protected ApiRequestContext ApiRequestContext { get; private set; } = null!;

    private static ApiRequestContext CreateApiRequestContext(string role)
    {
        return new ApiRequestContext
        {
            LoggedUser = new UserBuilder(Consts.UserId, Consts.Username + "-" + role, Consts.Discriminator).Build(),
            LoggedUserData = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, Consts.UserId.ToString())
            }))
        };
    }

    protected override void PreInit()
    {
        ApiRequestContext = IsPublic ? CreateApiRequestContext("User") : CreateApiRequestContext("Admin");
    }
}
