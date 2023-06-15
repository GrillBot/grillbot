using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;

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
