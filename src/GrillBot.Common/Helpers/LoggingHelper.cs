// ReSharper disable ConvertIfStatementToSwitchStatement ConvertIfStatementToReturnStatement

using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Common.Helpers;

public static class LoggingHelper
{
    public static bool IsWarning(string source, Exception ex)
    {
        var conditions = new Func<bool>[]
        {
            () => ex is GatewayReconnectException || ex.InnerException is GatewayReconnectException,
            () => ex.InnerException is null && ex.Message.StartsWith("Server missed last heartbeat", StringComparison.InvariantCultureIgnoreCase),
            () => ex is TaskCanceledException or HttpRequestException && ex.InnerException is IOException { InnerException: SocketException se } &&
                new[] { SocketError.TimedOut, SocketError.ConnectionAborted }.Contains(se.SocketErrorCode),
            // 11 is magic constant represents error "Resource temporarily unavailable".
            () => ex is HttpRequestException && ex.InnerException is SocketException { ErrorCode: 11 },
            () => ex.InnerException is WebSocketException or WebSocketClosedException,
            () => ex is TaskCanceledException && ex.InnerException is null,
            () => ex is HttpException { HttpCode: HttpStatusCode.ServiceUnavailable },
            () => ex is TimeoutException && (ex.Message.Contains("defer") || ex.Message.Contains("Cannot respond to an interaction after 3 seconds!")),
            () => source == "RemoveUnverify" && ex is DbUpdateConcurrencyException,
            () => ex is HttpException { DiscordCode: DiscordErrorCode.UnknownInteraction }
        };

        return Array.Exists(conditions, o => o());
    }
}
