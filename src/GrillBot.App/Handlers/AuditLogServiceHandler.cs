using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;

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

    protected static LogRequest CreateRequest(LogType type, IGuild? guild = null, IChannel? channel = null, IUser? user = null, IAuditLogEntry? auditLogEntry = null)
    {
        return new LogRequest
        {
            Type = type,
            ChannelId = channel?.Id.ToString(),
            CreatedAt = DateTime.UtcNow,
            DiscordId = auditLogEntry?.Id.ToString(),
            GuildId = guild?.Id.ToString(),
            UserId = user?.Id.ToString()
        };
    }
}
