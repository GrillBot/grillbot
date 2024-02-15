using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Handlers.RoleDeleted;

public class AuditRoleDeletedHandler : AuditLogServiceHandler, IRoleDeletedEvent
{
    public AuditRoleDeletedHandler(IAuditLogServiceClient client) : base(client)
    {
    }

    public async Task ProcessAsync(IRole role)
    {
        if (role.Guild is null)
            return;

        var request = CreateRequest(LogType.RoleDeleted, role.Guild);
        request.RoleDeleted = new RoleDeletedRequest { RoleId = role.Id.ToString() };

        await SendRequestAsync(request);
    }
}
