using AutoMapper;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Models;
using GrillBot.Database.Models;

namespace GrillBot.App.Services.AuditLog;

public class AuditLogApiService
{
    private FileStorageFactory FileStorage { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private ApiRequestContext ApiRequestContext { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public AuditLogApiService(GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, FileStorageFactory fileStorage,
        ApiRequestContext apiRequestContext, AuditLogWriter auditLogWriter)
    {
        FileStorage = fileStorage;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        ApiRequestContext = apiRequestContext;
        AuditLogWriter = auditLogWriter;
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
            () => IsValidFilter(item, AuditLogItemType.OverwriteCreated, parameters.TargetIdFilter),
            () => IsValidFilter(item, AuditLogItemType.OverwriteDeleted, parameters.TargetIdFilter),
            () => IsValidFilter(item, AuditLogItemType.OverwriteUpdated, parameters.TargetIdFilter),
            () => IsValidFilter(item, AuditLogItemType.MemberUpdated, parameters.TargetIdFilter),
            () => IsValidFilter(item, AuditLogItemType.MemberRoleUpdated, parameters.TargetIdFilter)
        };

        return conditions.Any(o => o());
    }

    private static bool IsValidFilter(AuditLogItem item, AuditLogItemType type, IExtendedFilter filter)
    {
        if (item.Type != type) return false; // Invalid type.
        return filter?.IsSet() != true || filter.IsValid(item, AuditLogWriter.SerializerSettings);
    }

    public async Task<PaginatedResponse<AuditLogListItem>> GetListAsync(AuditLogListParams parameters)
    {
        var logIds = await GetLogIdsAsync(parameters);

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.AuditLog.GetLogListAsync(parameters, parameters.Pagination, logIds);
        return await PaginatedResponse<AuditLogListItem>.CopyAndMapAsync(data, entity => Task.FromResult(MapItem(entity)));
    }

    private AuditLogListItem MapItem(AuditLogItem entity)
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

    public async Task<FileInfo> GetLogItemFileAsync(long logId, long fileId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var logItem = await repository.AuditLog.FindLogItemByIdAsync(logId, true);

        if (logItem == null)
            throw new NotFoundException("Požadovaný záznam v logu nebyl nalezen.");

        var fileEntity = logItem.Files.FirstOrDefault(o => o.Id == fileId);
        if (fileEntity == null)
            throw new NotFoundException("K tomuto záznamu neexistuje žádný záznam o existenci souboru.");

        var storage = FileStorage.Create("Audit");
        var file = await storage.GetFileInfoAsync("DeletedAttachments", fileEntity.Filename);

        if (!file.Exists)
            throw new NotFoundException("Hledaný soubor neexistuje na disku.");

        return file;
    }

    public async Task<bool> RemoveItemAsync(long id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var item = await repository.AuditLog.FindLogItemByIdAsync(id, true);

        if (item == null) return false;
        if (item.Files.Count > 0)
        {
            var storage = FileStorage.Create("Audit");

            foreach (var file in item.Files)
            {
                var fileInfo = await storage.GetFileInfoAsync("DeletedAttachments", file.Filename);
                if (!fileInfo.Exists) continue;

                fileInfo.Delete();
            }

            repository.RemoveCollection(item.Files);
        }

        repository.Remove(item);
        return await repository.CommitAsync() > 0;
    }

    public async Task HandleClientAppMessageAsync(ClientLogItemRequest request)
    {
        var item = new AuditLogDataWrapper(request.GetAuditLogType(), request.Content, processedUser: ApiRequestContext.LoggedUser);
        await AuditLogWriter.StoreAsync(item);
    }
}
