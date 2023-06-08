using AuditLogService.Models.Request;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;

namespace GrillBot.App.Handlers.GuildUpdated;

public class AuditEmotesGuildUpdatedHandler : AuditLogServiceHandler, IGuildUpdatedEvent
{
    public AuditEmotesGuildUpdatedHandler(IAuditLogServiceClient auditLogServiceClient) : base(auditLogServiceClient)
    {
    }

    public async Task ProcessAsync(IGuild before, IGuild after)
    {
        var removedEmotes = before.Emotes.Where(e => !after.Emotes.Contains(e)).ToList();
        if (removedEmotes.Count == 0)
            return;

        var requests = new List<LogRequest>();
        foreach (var emote in removedEmotes)
        {
            var request = CreateRequest(LogType.EmoteDeleted, after);
            request.DeletedEmote = new DeletedEmoteRequest { EmoteId = emote.ToString() };

            requests.Add(request);
        }

        await SendRequestsAsync(requests);
    }
}
