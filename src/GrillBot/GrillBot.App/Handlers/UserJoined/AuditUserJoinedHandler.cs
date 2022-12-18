using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.UserJoined;

public class AuditUserJoinedHandler : IUserJoinedEvent
{
    private AuditLogWriteManager AuditLogWriteManager { get; }

    public AuditUserJoinedHandler(AuditLogWriteManager auditLogWriteManager)
    {
        AuditLogWriteManager = auditLogWriteManager;
    }

    public async Task ProcessAsync(IGuildUser user)
    {
        if (!user.IsUser() || user.Guild is not SocketGuild guild) return;

        var data = new UserJoinedAuditData(guild);
        var item = new AuditLogDataWrapper(AuditLogItemType.UserJoined, data, guild, processedUser: user);
        await AuditLogWriteManager.StoreAsync(item);
    }
}
