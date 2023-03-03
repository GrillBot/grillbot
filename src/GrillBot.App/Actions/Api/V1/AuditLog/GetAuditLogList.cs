using AutoMapper;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Common.Models.Pagination;
using GrillBot.Common.Services.FileService;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.System;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class GetAuditLogList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private ITextsManager Texts { get; }
    private FileStorageFactory FileStorageFactory { get; }
    private IFileServiceClient FileServiceClient { get; }

    public GetAuditLogList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, ITextsManager texts, FileStorageFactory fileStorageFactory,
        IFileServiceClient fileServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        Texts = texts;
        FileStorageFactory = fileStorageFactory;
        FileServiceClient = fileServiceClient;
    }

    public async Task<PaginatedResponse<AuditLogListItem>> ProcessAsync(AuditLogListParams parameters)
    {
        ValidateParameters(parameters);
        parameters.UpdateStartDate(new DiagnosticsInfo().StartAt);
        var logIds = await GetLogIdsAsync(parameters);

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.AuditLog.GetLogListAsync(parameters, parameters.Pagination, logIds);
        return await PaginatedResponse<AuditLogListItem>.CopyAndMapAsync(data, MapAsync);
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

                throw new ValidationException(Texts["AuditLog/List/IdNotNumber", ApiContext.Language].FormatWith(i)).ToBadRequestValidation(items[i], $"{nameof(parameters.Ids)}[{i}]");
            }
        }

        if (parameters.Types.Count == 0 || parameters.ExcludedTypes.Count == 0) return;

        var intersectTypes = parameters.ExcludedTypes.Intersect(parameters.Types);
        if (!intersectTypes.Any())
            return;

        throw new ValidationException(Texts["AuditLog/List/TypesCombination", ApiContext.Language]).ToBadRequestValidation(intersectTypes, nameof(parameters.ExcludedTypes), nameof(parameters.Types));
    }

    private async Task<List<long>?> GetLogIdsAsync(AuditLogListParams parameters)
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
        var conditions = new[]
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
            () => IsValidFilter(item, AuditLogItemType.MemberRoleUpdated, parameters.MemberRolesUpdatedFilter),
            () => IsValidFilter(item, AuditLogItemType.MessageDeleted, parameters.MessageDeletedFilter)
        };

        return conditions.Any(o => o());
    }

    private static bool IsValidFilter(AuditLogItem item, AuditLogItemType type, IExtendedFilter? filter)
    {
        if (item.Type != type) return false; // Invalid type.
        if (filter == null || !filter.IsSet()) return true;
        return filter.IsValid(item, AuditLogWriteManager.SerializerSettings);
    }

    private async Task<AuditLogListItem> MapAsync(AuditLogItem entity)
    {
        var mapped = Mapper.Map<AuditLogListItem>(entity);
        if (string.IsNullOrEmpty(entity.Data))
            return mapped;

        mapped.Data = entity.Type switch
        {
            AuditLogItemType.Error or AuditLogItemType.Info or AuditLogItemType.Warning => entity.Data,
            AuditLogItemType.Command => JsonConvert.DeserializeObject<CommandExecution>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.ChannelCreated or AuditLogItemType.ChannelDeleted => JsonConvert.DeserializeObject<AuditChannelInfo>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.ChannelUpdated => JsonConvert.DeserializeObject<Diff<AuditChannelInfo>>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.EmojiDeleted => JsonConvert.DeserializeObject<AuditEmoteInfo>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.GuildUpdated => JsonConvert.DeserializeObject<GuildUpdatedData>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.MemberRoleUpdated or AuditLogItemType.MemberUpdated => JsonConvert.DeserializeObject<MemberUpdatedData>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.MessageDeleted => JsonConvert.DeserializeObject<MessageDeletedData>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.MessageEdited => JsonConvert.DeserializeObject<MessageEditedData>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.OverwriteCreated or AuditLogItemType.OverwriteDeleted => JsonConvert.DeserializeObject<AuditOverwriteInfo>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.OverwriteUpdated => JsonConvert.DeserializeObject<Diff<AuditOverwriteInfo>>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.Unban => JsonConvert.DeserializeObject<AuditUserInfo>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.UserJoined => JsonConvert.DeserializeObject<UserJoinedAuditData>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.UserLeft => JsonConvert.DeserializeObject<UserLeftGuildData>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.InteractionCommand => JsonConvert.DeserializeObject<InteractionCommandExecuted>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.ThreadDeleted => JsonConvert.DeserializeObject<AuditThreadInfo>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.JobCompleted => JsonConvert.DeserializeObject<JobExecutionData>(entity.Data, AuditLogWriteManager.SerializerSettings),
            AuditLogItemType.Api => JsonConvert.DeserializeObject<ApiRequest>(entity.Data, AuditLogWriteManager.SerializerSettings),
            _ => null
        };

        await MapFilesAsync(mapped);
        return mapped;
    }

    private async Task MapFilesAsync(AuditLogListItem listItem)
    {
        if (listItem.Files.Count == 0) return;
        var storage = FileStorageFactory.Create("Audit");

        foreach (var file in listItem.Files)
        {
            var localInfo = await storage.GetFileInfoAsync("DeletedAttachments", file.Filename);
            if (!localInfo.Exists)
                file.SasLink = await FileServiceClient.GenerateLinkAsync(file.Filename);
        }
    }
}
