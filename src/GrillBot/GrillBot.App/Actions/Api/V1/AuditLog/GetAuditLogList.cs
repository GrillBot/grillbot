using AutoMapper;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.System;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Models;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class GetAuditLogList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private ITextsManager Texts { get; }

    public GetAuditLogList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        Texts = texts;
    }

    public async Task<PaginatedResponse<AuditLogListItem>> ProcessAsync(AuditLogListParams parameters)
    {
        ValidateParameters(parameters);
        parameters.UpdateStartDate(new DiagnosticsInfo().StartAt);
        var logIds = await GetLogIdsAsync(parameters);

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.AuditLog.GetLogListAsync(parameters, parameters.Pagination, logIds);
        return await PaginatedResponse<AuditLogListItem>.CopyAndMapAsync(data, entity => Task.FromResult(Map(entity)));
    }

    private void ValidateParameters(AuditLogListParams parameters)
    {
        if (!string.IsNullOrEmpty(parameters.Ids))
        {
            var items = parameters.Ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (var i = 0; i < items.Length; i++)
            {
                if (long.TryParse(items[i], out _))
                    continue;

                var text = Texts["AuditLog/List/IdNotNumber", ApiContext.Language].FormatWith(i);
                var result = new ValidationResult(text, new[] { $"{nameof(parameters.Ids)}[{i}]" });
                throw new ValidationException(result, null, items[i]);
            }
        }

        if (parameters.Types.Count == 0 || parameters.ExcludedTypes.Count == 0) return;

        var intersectTypes = parameters.ExcludedTypes.Intersect(parameters.Types);
        if (!intersectTypes.Any())
            return;

        throw new ValidationException(
            new ValidationResult(Texts["AuditLog/List/TypesCombination", ApiContext.Language], new[] { nameof(parameters.ExcludedTypes), nameof(parameters.Types) }), null, intersectTypes);
    }

    private async Task<List<long>> GetLogIdsAsync(AuditLogListParams parameters)
    {
        if (!parameters.AnyExtendedFilter())
            return null; // Log ids could get only if some extended filter was set.

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.AuditLog.GetSimpleDataAsync(parameters);
        return data
            .Where(o => IsValidExtendedFilter(parameters, o))
            .Select(o => o.Id)
            .ToList();
    }

    private static bool IsValidExtendedFilter(AuditLogListParams parameters, AuditLogItem item)
    {
        var conditions = new Func<bool>[]
        {
            () => IsValidFilter(item, AuditLogItemType.Info, parameters.InfoFilter),
            () => IsValidFilter(item, AuditLogItemType.Warning, parameters.WarningFilter),
            () => IsValidFilter(item, AuditLogItemType.Error, parameters.ErrorFilter),
            () => IsValidFilter(item, AuditLogItemType.Command, parameters.CommandFilter),
            () => IsValidFilter(item, AuditLogItemType.InteractionCommand, parameters.InteractionFilter),
            () => IsValidFilter(item, AuditLogItemType.JobCompleted, parameters.JobFilter),
            () => IsValidFilter(item, AuditLogItemType.Api, parameters.ApiRequestFilter),
            () => IsValidFilter(item, AuditLogItemType.OverwriteCreated, parameters.OverwriteCreatedFilter),
            () => IsValidFilter(item, AuditLogItemType.OverwriteDeleted, parameters.OverwriteDeletedFilter),
            () => IsValidFilter(item, AuditLogItemType.OverwriteUpdated, parameters.OverwriteUpdatedFilter),
            () => IsValidFilter(item, AuditLogItemType.MemberUpdated, parameters.MemberUpdatedFilter),
            () => IsValidFilter(item, AuditLogItemType.MemberRoleUpdated, parameters.MemberRolesUpdatedFilter)
        };

        return conditions.Any(o => o());
    }

    private static bool IsValidFilter(AuditLogItem item, AuditLogItemType type, IExtendedFilter filter)
    {
        if (item.Type != type) return false; // Invalid type.
        if (filter == null || !filter.IsSet()) return true;
        return filter.IsValid(item, AuditLogWriter.SerializerSettings);
    }

    private AuditLogListItem Map(AuditLogItem entity)
    {
        var mapped = Mapper.Map<AuditLogListItem>(entity);
        if (string.IsNullOrEmpty(entity.Data))
            return mapped;

        mapped.Data = entity.Type switch
        {
            AuditLogItemType.Error or AuditLogItemType.Info or AuditLogItemType.Warning => entity.Data,
            AuditLogItemType.Command => JsonConvert.DeserializeObject<CommandExecution>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.ChannelCreated or AuditLogItemType.ChannelDeleted => JsonConvert.DeserializeObject<AuditChannelInfo>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.ChannelUpdated => JsonConvert.DeserializeObject<Diff<AuditChannelInfo>>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.EmojiDeleted => JsonConvert.DeserializeObject<AuditEmoteInfo>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.GuildUpdated => JsonConvert.DeserializeObject<GuildUpdatedData>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.MemberRoleUpdated or AuditLogItemType.MemberUpdated => JsonConvert.DeserializeObject<MemberUpdatedData>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.MessageDeleted => JsonConvert.DeserializeObject<MessageDeletedData>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.MessageEdited => JsonConvert.DeserializeObject<MessageEditedData>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.OverwriteCreated or AuditLogItemType.OverwriteDeleted => JsonConvert.DeserializeObject<AuditOverwriteInfo>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.OverwriteUpdated => JsonConvert.DeserializeObject<Diff<AuditOverwriteInfo>>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.Unban => JsonConvert.DeserializeObject<AuditUserInfo>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.UserJoined => JsonConvert.DeserializeObject<UserJoinedAuditData>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.UserLeft => JsonConvert.DeserializeObject<UserLeftGuildData>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.InteractionCommand => JsonConvert.DeserializeObject<InteractionCommandExecuted>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.ThreadDeleted => JsonConvert.DeserializeObject<AuditThreadInfo>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.JobCompleted => JsonConvert.DeserializeObject<JobExecutionData>(entity.Data, AuditLogWriter.SerializerSettings),
            AuditLogItemType.Api => JsonConvert.DeserializeObject<ApiRequest>(entity.Data, AuditLogWriter.SerializerSettings),
            _ => null
        };

        return mapped;
    }
}
