﻿using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;

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

    public override async Task<ApiResult> ProcessAsync()
    {
        var emoteId = (string)Parameters[0]!;

        await using var repository = DatabaseBuilder.CreateRepository();

        var emotes = await repository.Emote.FindStatisticsByEmoteIdAsync(emoteId);
        if (emotes.Count == 0)
            return ApiResult.Ok(0);

        await WriteToAuditlogAsync(emoteId, emotes.Count);
        repository.RemoveCollection(emotes);
        var result = await repository.CommitAsync();

        return ApiResult.Ok(result);
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
                Severity = LogSeverity.Info,
                SourceAppName = "GrillBot",
                Source = $"Emote.{nameof(RemoveStats)}"
            },
            UserId = ApiContext.GetUserId().ToString()
        };

        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
    }
}
