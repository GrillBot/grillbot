using Discord.Net;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public partial class AuditOrchestrationHandler
{
    private async Task<List<(IAuditLogEntry entry, TData data)>> ReadAuditLogsAsync<TData>(IGuild guild, ActionType actionType)
    {
        IReadOnlyCollection<IAuditLogEntry> entries;
        using (_counterManager.Create("Discord.API.AuditLog"))
        {
            try
            {
                entries = await guild.GetAuditLogsAsync(DiscordConfig.MaxAuditLogEntriesPerBatch, CacheMode.AllowDownload, actionType: actionType);
            }
            catch (HttpException ex) when (ex.IsExpectedOutageError())
            {
                entries = new List<IAuditLogEntry>().AsReadOnly();
            }
        }

        return entries.Select(e => (e, (TData)e.Data)).ToList();
    }

    private Task PushPayloadAsync(CreateItemsPayload payload)
        => payload.Items.Count > 0 ? _rabbitPublisher.PublishAsync(payload, new()) : Task.CompletedTask;

    private Task PushPayloadAsync(params LogRequest[] requests)
        => PushPayloadAsync(new CreateItemsPayload(requests.ToList()));
}
