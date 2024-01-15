using Discord.Net;
using System.Net;

namespace GrillBot.Common.Extensions.Discord;

public static class HttpExceptionExtensions
{
    public static bool IsExpectedOutageError(this HttpException ex)
        => ex.HttpCode == HttpStatusCode.InternalServerError || ex.HttpCode == HttpStatusCode.ServiceUnavailable;
}
