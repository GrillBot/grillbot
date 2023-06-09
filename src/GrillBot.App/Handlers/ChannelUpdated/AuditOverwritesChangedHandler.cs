using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Managers;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.App.Handlers.ChannelUpdated;

public class AuditOverwritesChangedHandler : AuditLogServiceHandler, IChannelUpdatedEvent
{
    private AuditLogManager AuditLogManager { get; }
    private ICounterManager CounterManager { get; }

    public AuditOverwritesChangedHandler(AuditLogManager auditLogManager, ICounterManager counterManager, IAuditLogServiceClient auditLogServiceClient) : base(auditLogServiceClient)
    {
        AuditLogManager = auditLogManager;
        CounterManager = counterManager;
    }

    public async Task ProcessAsync(IChannel before, IChannel after)
    {
        if (!Init(after, out var guildChannel)) return;
        AuditLogManager.OnOverwriteChangedEvent(after.Id, DateTime.Now.AddMinutes(1));

        var requests = new List<LogRequest>();
        var auditLogs = await GetAuditLogsAsync(guildChannel, ActionType.OverwriteCreated);
        requests.AddRange(auditLogs.Select(entry => CreateRequest(LogType.OverwriteCreated, guildChannel.Guild, guildChannel, null, entry)));

        auditLogs = await GetAuditLogsAsync(guildChannel, ActionType.OverwriteDeleted);
        requests.AddRange(auditLogs.Select(entry => CreateRequest(LogType.OverwriteDeleted, guildChannel.Guild, guildChannel, null, entry)));

        auditLogs = await GetAuditLogsAsync(guildChannel, ActionType.OverwriteUpdated);
        requests.AddRange(auditLogs.Select(entry => CreateRequest(LogType.OverwriteUpdated, guildChannel.Guild, guildChannel, null, entry)));

        if (requests.Count > 0)
            await SendRequestsAsync(requests);
    }

    private bool Init(IChannel channel, [MaybeNullWhen(false)] out IGuildChannel guildChannelAfter)
    {
        guildChannelAfter = channel as IGuildChannel;

        if (guildChannelAfter == null) return false;
        return AuditLogManager.GetNextOverwriteEvent(channel.Id) <= DateTime.Now;
    }

    private async Task<List<IAuditLogEntry>> GetAuditLogsAsync(IGuildChannel channel, ActionType actionType)
    {
        IReadOnlyCollection<IAuditLogEntry> auditLogs;
        using (CounterManager.Create("Discord.API.AuditLog"))
        {
            auditLogs = await channel.Guild.GetAuditLogsAsync(actionType: actionType);
        }

        return auditLogs
            .Where(o => IsValidEntry(o, channel))
            .ToList();
    }

    private static bool IsValidEntry(IAuditLogEntry entry, IGuildChannel channel)
    {
        return entry.Action switch
        {
            ActionType.OverwriteCreated => ((OverwriteCreateAuditLogData)entry.Data).ChannelId == channel.Id,
            ActionType.OverwriteDeleted => ((OverwriteDeleteAuditLogData)entry.Data).ChannelId == channel.Id,
            ActionType.OverwriteUpdated => ((OverwriteUpdateAuditLogData)entry.Data).ChannelId == channel.Id,
            _ => false
        };
    }
}
