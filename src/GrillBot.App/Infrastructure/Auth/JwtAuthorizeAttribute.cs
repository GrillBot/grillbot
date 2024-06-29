using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace GrillBot.App.Infrastructure.Auth;

public class JwtAuthorizeAttribute : AuthorizeAttribute
{
    public string[] RequiredPermissions { get; }

    public JwtAuthorizeAttribute(params string[] requiredPermissions) : base("")
    {
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
        RequiredPermissions = requiredPermissions;
    }
}
