using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class UserJoinedEvent : AuditEventBase
{
    private SocketGuildUser User { get; }

    public UserJoinedEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, SocketGuildUser user) : base(auditLogService, auditLogWriter)
    {
        User = user;
    }

    public override Task<bool> CanProcessAsync() =>
        Task.FromResult(User?.IsUser() == true);

    public override async Task ProcessAsync()
    {
        var data = new UserJoinedAuditData(User.Guild);
        var item = new AuditLogDataWrapper(AuditLogItemType.UserJoined, data, User.Guild, processedUser: User);

        await AuditLogWriter.StoreAsync(item);
    }
}
