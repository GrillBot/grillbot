using Microsoft.AspNetCore.Http;

namespace GrillBot.Common.Extensions;

public static class HttpContextExtensions
{
    public static string GetRemoteIp(this HttpContext context)
    {
        var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        string remoteIp;
        if (!string.IsNullOrEmpty(forwardedHeader))
            remoteIp = forwardedHeader.Split(',')[0].Trim();
        else
            remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        return remoteIp == "::1" ? "127.0.0.1" : remoteIp;
    }
}
