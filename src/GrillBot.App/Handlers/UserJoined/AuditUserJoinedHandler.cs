using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;

namespace GrillBot.App.Handlers.UserJoined;

public class AuditUserJoinedHandler : AuditLogServiceHandler, IUserJoinedEvent
{
    public AuditUserJoinedHandler(IAuditLogServiceClient client) : base(client)
    {
    }

    public async Task ProcessAsync(IGuildUser user)
    {
        if (!user.IsUser() || user.Guild is not SocketGuild guild) return;

        var request = CreateRequest(LogType.UserJoined, user.Guild, null, user);
        request.UserJoined = new UserJoinedRequest { MemberCount = guild.MemberCount };

        await SendRequestAsync(request);
    }
}
