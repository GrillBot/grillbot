﻿using GrillBot.App.Managers;
using GrillBot.Common.Models;
using GrillBot.Core.Managers.Discord;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class MergeStats : ApiAction
{
    private IEmoteManager EmoteManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }

    public MergeStats(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, AuditLogWriteManager auditLogWriteManager, IEmoteManager emoteManager) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        AuditLogWriteManager = auditLogWriteManager;
        EmoteManager = emoteManager;
    }

    public async Task<int> ProcessAsync(MergeEmoteStatsParams parameters)
    {
        await ValidateMergeAsync(parameters);
        await using var repository = DatabaseBuilder.CreateRepository();

        var sourceStats = await repository.Emote.FindStatisticsByEmoteIdAsync(parameters.SourceEmoteId);
        if (sourceStats.Count == 0) return 0;

        var destinationStats = await repository.Emote.FindStatisticsByEmoteIdAsync(parameters.DestinationEmoteId);
        foreach (var item in sourceStats)
        {
            var destinationStatItem = destinationStats.Find(o => o.UserId == item.UserId && o.GuildId == item.GuildId);
            if (destinationStatItem == null)
            {
                destinationStatItem = new Database.Entity.EmoteStatisticItem
                {
                    EmoteId = parameters.DestinationEmoteId,
                    GuildId = item.GuildId,
                    UserId = item.UserId
                };

                await repository.AddAsync(destinationStatItem);
            }

            if (item.LastOccurence > destinationStatItem.LastOccurence)
                destinationStatItem.LastOccurence = item.LastOccurence;

            if (item.FirstOccurence != DateTime.MinValue && (item.FirstOccurence < destinationStatItem.FirstOccurence || destinationStatItem.FirstOccurence == DateTime.MinValue))
                destinationStatItem.FirstOccurence = item.FirstOccurence;

            destinationStatItem.UseCount += item.UseCount;
            repository.Remove(item);
        }

        var logItem = new AuditLogDataWrapper(AuditLogItemType.Info,
            $"Provedeno sloučení emotů {parameters.SourceEmoteId} do {parameters.DestinationEmoteId}. Sloučeno záznamů: {sourceStats.Count}/{destinationStats.Count}",
            null, null, ApiContext.LoggedUser);
        await AuditLogWriteManager.StoreAsync(logItem);
        return await repository.CommitAsync();
    }

    private async Task ValidateMergeAsync(MergeEmoteStatsParams @params)
    {
        var supportedEmotes = (await EmoteManager.GetSupportedEmotesAsync()).ConvertAll(o => o.ToString());
        if (!supportedEmotes.Contains(@params.DestinationEmoteId))
        {
            throw new ValidationException(
                new ValidationResult("Nelze sloučit statistiku do neexistujícího emotu.", new[] { nameof(@params.DestinationEmoteId) }), null, @params.DestinationEmoteId
            );
        }
    }
}
