using GrillBot.Common.Models;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class RemoveStats : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public RemoveStats(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        AuditLogServiceClient = auditLogServiceClient;
    }

    public async Task<int> ProcessAsync(string emoteId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var emotes = await repository.Emote.FindStatisticsByEmoteIdAsync(emoteId);
        if (emotes.Count == 0) return 0;

        await WriteToAuditlogAsync(emoteId, emotes.Count);
        repository.RemoveCollection(emotes);
        return await repository.CommitAsync();
    }

    private async Task WriteToAuditlogAsync(string emoteId, int emotesCount)
    {
        var logRequest = new LogRequest
        {
            Type = LogType.Info,
            CreatedAt = DateTime.UtcNow,
            LogMessage = new LogMessageRequest
            {
                Message = $"Statistiky emotu {emoteId} byly smazány. Smazáno záznamů: {emotesCount}",
                Severity = LogSeverity.Info
            },
            UserId = ApiContext.GetUserId().ToString()
        };

        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
    }
}
