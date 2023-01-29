using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;
using GrillBot.App.Actions;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.Infrastructure.Common;

[ExcludeFromCodeCoverage]
public abstract class ApiActionTest<TAction> : ActionTest<TAction> where TAction : ApiAction
{
    private ApiConfigurationAttribute ApiConfiguration
        => GetMethod().GetCustomAttribute<ApiConfigurationAttribute>();

    protected bool IsPublic => ApiConfiguration?.IsPublic ?? false;

    private static readonly Lazy<ApiRequestContext> UserApiRequestContext
        = new(() => CreateApiRequestContext("User"), LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Lazy<ApiRequestContext> AdminApiRequestContext
        = new(() => CreateApiRequestContext("Admin"), LazyThreadSafetyMode.ExecutionAndPublication);

    protected ApiRequestContext ApiRequestContext { get; private set; }

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

    protected override void Init()
    {
        ApiRequestContext = IsPublic ? UserApiRequestContext.Value : AdminApiRequestContext.Value;
    }
}
