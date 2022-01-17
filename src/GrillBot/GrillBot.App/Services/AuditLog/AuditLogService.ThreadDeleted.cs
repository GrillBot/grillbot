using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable S1172 // Unused method parameters should be removed
namespace GrillBot.Data.Services.AuditLog;

public partial class AuditLogService
{
    private Task<bool> CanProcessThreadDeletedAsync(Cacheable<SocketThreadChannel, ulong> _)
        => Task.FromResult(InitializationService.Get());

    public async Task ProcessThreadDeletedAsync(Cacheable<SocketThreadChannel, ulong> cachedThread)
    {
        var thread = await cachedThread.GetOrDownloadAsync();
        var guild = await GetGuildFromChannelAsync(thread, cachedThread.Id);
        if (guild == null) return;

        var auditLog = (await guild.GetAuditLogsAsync(actionType: ActionType.ThreadDelete))
            .FirstOrDefault(o => cachedThread.Id == ((ThreadDeleteAuditLogData)o.Data).ThreadId);
        if (auditLog == null) return;

        var data = new AuditThreadInfo(auditLog.Data as ThreadDeleteAuditLogData);
        var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);

        await StoreItemAsync(AuditLogItemType.ThreadDeleted, guild, thread, auditLog.User, json, auditLog.Id);
    }
}
