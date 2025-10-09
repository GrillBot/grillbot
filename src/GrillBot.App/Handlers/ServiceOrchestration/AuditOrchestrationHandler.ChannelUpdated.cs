using GrillBot.Common.Extensions.Discord;
using AuditLog.Enums;
using AuditLog.Models.Events.Create;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public partial class AuditOrchestrationHandler
{
    private static void ProcessChannelChanges(IChannel before, IChannel after, CreateItemsMessage payload)
    {
        if (before is not IGuildChannel beforeGuildChannel) return;
        if (after is not IGuildChannel afterGuildChannel) return;
        if (beforeGuildChannel.IsEqual(afterGuildChannel)) return;

        var guildId = afterGuildChannel.GuildId.ToString();
        var channelId = after.Id.ToString();

        payload.Items.Add(new LogRequest(LogType.ChannelUpdated, DateTime.UtcNow, guildId, null, channelId)
        {
            ChannelUpdated = new DiffRequest<ChannelInfoRequest>
            {
                After = new ChannelInfoRequest
                {
                    Position = afterGuildChannel.Position,
                    Topic = (afterGuildChannel as ITextChannel)?.Topic
                },
                Before = new ChannelInfoRequest
                {
                    Position = beforeGuildChannel.Position,
                    Topic = (beforeGuildChannel as ITextChannel)?.Topic
                }
            }
        });
    }

    private async Task ProcessOverwriteChangesAsync(IChannel after, CreateItemsMessage payload)
    {
        if (after is not IGuildChannel guildChannel) return;
        if (!_auditLogManager.CanProcessNextOverwriteEvent(after.Id)) return;

        _auditLogManager.OnOverwriteChangedEvent(after.Id, DateTime.Now.AddMinutes(1));

        var guildId = guildChannel.GuildId.ToString();
        var channelId = after.Id.ToString();

        var overwriteCreatedLogs = await ReadAuditLogsAsync<OverwriteCreateAuditLogData>(guildChannel.Guild, ActionType.OverwriteCreated);
        foreach (var (entry, _) in overwriteCreatedLogs.Where(o => o.data.ChannelId == after.Id))
            payload.Items.Add(new LogRequest(LogType.OverwriteCreated, entry.CreatedAt.UtcDateTime, guildId, entry.User.Id.ToString(), channelId, entry.Id.ToString()));

        var overwriteUpdatedLogs = await ReadAuditLogsAsync<OverwriteUpdateAuditLogData>(guildChannel.Guild, ActionType.OverwriteUpdated);
        foreach (var (entry, _) in overwriteUpdatedLogs.Where(o => o.data.ChannelId == after.Id))
            payload.Items.Add(new LogRequest(LogType.OverwriteUpdated, entry.CreatedAt.UtcDateTime, guildId, entry.User.Id.ToString(), channelId, entry.Id.ToString()));

        var overwriteDeletedLogs = await ReadAuditLogsAsync<OverwriteDeleteAuditLogData>(guildChannel.Guild, ActionType.OverwriteDeleted);
        foreach (var (entry, _) in overwriteDeletedLogs.Where(o => o.data.ChannelId == after.Id))
            payload.Items.Add(new LogRequest(LogType.OverwriteDeleted, entry.CreatedAt.UtcDateTime, guildId, entry.User.Id.ToString(), channelId, entry.Id.ToString()));
    }
}
