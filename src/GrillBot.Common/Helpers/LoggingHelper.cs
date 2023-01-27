// ReSharper disable ConvertIfStatementToSwitchStatement ConvertIfStatementToReturnStatement

using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using Discord.Net;
using Discord.WebSocket;

namespace GrillBot.Common.Helpers;

public static class LoggingHelper
{
    public static bool IsWarning(Exception ex)
    {
        if (ex is GatewayReconnectException || ex.InnerException is GatewayReconnectException) return true;
        if (ex.InnerException == null && ex.Message.StartsWith("Server missed last heartbeat", StringComparison.InvariantCultureIgnoreCase)) return true;
        if (ex is TaskCanceledException or HttpRequestException && ex.InnerException is IOException { InnerException: SocketException se } &&
            new[] { SocketError.TimedOut, SocketError.ConnectionAborted }.Contains(se.SocketErrorCode)) return true;
        // 11 is magic constant represents error "Resource temporarily unavailable".
        if (ex is HttpRequestException && ex.InnerException is SocketException { ErrorCode: 11 }) return true;
        if (ex.InnerException is WebSocketException or WebSocketClosedException) return true;
        if (ex is TaskCanceledException && ex.InnerException is null) return true;
        if (ex is HttpException { HttpCode: HttpStatusCode.ServiceUnavailable }) return true;
        if (ex is TimeoutException && ex.Message.Contains("defer")) return true;

        return false;
    }
}
