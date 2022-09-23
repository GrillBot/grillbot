using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Emotes;

public class EmotesApiService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ApiRequestContext ApiRequestContext { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public EmotesApiService(GrillBotDatabaseBuilder databaseBuilder, ApiRequestContext apiRequestContext, AuditLogWriter auditLogWriter)
    {
        DatabaseBuilder = databaseBuilder;
        ApiRequestContext = apiRequestContext;
        AuditLogWriter = auditLogWriter;
    }

    public async Task<int> RemoveStatisticsAsync(string emoteId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var emotes = await repository.Emote.FindStatisticsByEmoteIdAsync(emoteId);

        if (emotes.Count == 0)
            return 0;

        var auditLogItem = new AuditLogDataWrapper(AuditLogItemType.Info,
            $"Statistiky emotu {emoteId} byly smazány. Smazáno záznamů: {emotes.Count}", null, null,
            ApiRequestContext.LoggedUser);
        await AuditLogWriter.StoreAsync(auditLogItem);

        repository.RemoveCollection(emotes);
        return await repository.CommitAsync();
    }
}
