using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class UserJoinedEvent : AuditEventBase
{
    private SocketGuildUser User { get; }

    public UserJoinedEvent(AuditLogService auditLogService, SocketGuildUser user) : base(auditLogService)
    {
        User = user;
    }

    public override Task<bool> CanProcessAsync() =>
        Task.FromResult(User?.IsUser() == true);

    public override async Task ProcessAsync()
    {
        var data = new UserJoinedAuditData(User.Guild);
        var jsonData = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings);
        await AuditLogService.StoreItemAsync(AuditLogItemType.UserJoined, User.Guild, null, User, jsonData);
    }
}
