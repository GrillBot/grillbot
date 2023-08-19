using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Handlers.GuildMemberUpdated;

public class AuditUserUpdatedHandler : AuditLogServiceHandler, IGuildMemberUpdatedEvent
{
    public AuditUserUpdatedHandler(IAuditLogServiceClient client) : base(client)
    {
    }

    public async Task ProcessAsync(IGuildUser? before, IGuildUser after)
    {
        if (before is null || !CanProcess(before, after)) return;

        var request = CreateRequest(LogType.MemberUpdated, after.Guild);
        request.MemberUpdated = new MemberUpdatedRequest { UserId = after.Id.ToString() };

        await SendRequestAsync(request);
    }

    private static bool CanProcess(IGuildUser before, IGuildUser after)
        => before.IsDeafened != after.IsDeafened || before.IsMuted != after.IsMuted || before.Nickname != after.Nickname;
}
