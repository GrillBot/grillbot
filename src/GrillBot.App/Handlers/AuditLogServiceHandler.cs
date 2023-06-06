using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;

namespace GrillBot.App.Handlers;

public abstract class AuditLogServiceHandler
{
    private IAuditLogServiceClient Client { get; }

    protected AuditLogServiceHandler(IAuditLogServiceClient client)
    {
        Client = client;
    }

    protected async Task SendRequestAsync(LogRequest request)
        => await SendRequestsAsync(new List<LogRequest> { request });

    protected async Task SendRequestsAsync(List<LogRequest> requests)
        => await Client.CreateItemsAsync(requests);

    protected static LogRequest CreateRequest(LogType type, ulong? guildId = null, ulong? channelId = null, ulong? userId = null, ulong? discordId = null)
    {
        return new LogRequest
        {
            Type = type,
            ChannelId = channelId?.ToString(),
            CreatedAt = DateTime.UtcNow,
            DiscordId = discordId?.ToString(),
            GuildId = guildId?.ToString(),
            UserId = userId?.ToString()
        };
    }
}
