using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Handlers.UserLeft;

public class AuditUserLeftHandler : AuditLogServiceHandler, IUserLeftEvent
{
    public AuditUserLeftHandler(IAuditLogServiceClient auditLogServiceClient) : base(auditLogServiceClient)
    {
    }

    public async Task ProcessAsync(IGuild guild, IUser user)
    {
        if (!await CanProcessAsync(guild, user))
            return;

        var request = CreateRequest(LogType.UserLeft, guild, null, user);
        request.UserLeft = new UserLeftRequest
        {
            UserId = user.Id.ToString(),
            MemberCount = Convert.ToInt32(guild.GetMemberCount())
        };

        await SendRequestAsync(request);
    }

    private static async Task<bool> CanProcessAsync(IGuild guild, IUser user)
    {
        var currentUser = await guild.GetCurrentUserAsync();
        return user.Id != currentUser.Id && currentUser.GuildPermissions is { ViewAuditLog: true, BanMembers: true };
    }
}
