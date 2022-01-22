using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog;

public partial class AuditLogService
{
    private Task<bool> CanProcessUserJoined(SocketGuildUser user)
        => Task.FromResult(user?.IsUser() == true);

    private async Task ProcessUserJoinedAsync(SocketGuildUser user)
    {
        var data = new UserJoinedAuditData(user.Guild);
        var jsonData = JsonConvert.SerializeObject(data, JsonSerializerSettings);
        await StoreItemAsync(AuditLogItemType.UserJoined, user.Guild, null, user, jsonData);
    }
}
