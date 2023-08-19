using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Handlers.UserUnbanned;

public class AuditUserUnbannedHandler : AuditLogServiceHandler, IUserUnbannedEvent
{
    public AuditUserUnbannedHandler(IAuditLogServiceClient client) : base(client)
    {
    }

    public async Task ProcessAsync(IUser user, IGuild guild)
    {
        var request = CreateRequest(LogType.Unban, guild);
        request.Unban = new UnbanRequest { UserId = user.Id.ToString() };

        await SendRequestAsync(request);
    }
}
