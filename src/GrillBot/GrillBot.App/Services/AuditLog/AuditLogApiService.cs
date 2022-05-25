using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.FileStorage;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog;

public class AuditLogApiService : ServiceBase
{
    private static JsonSerializerSettings JsonSerializerSettings
        => AuditLogService.JsonSerializerSettings;

    private FileStorageFactory FileStorage { get; }

    public AuditLogApiService(GrillBotContextFactory dbFactory, IMapper mapper, FileStorageFactory fileStorage) : base(null, dbFactory, null, mapper)
    {
        FileStorage = fileStorage;
    }

    private async Task<List<long>> GetLogIdsAsync(AuditLogListParams parameters, CancellationToken cancellationToken = default)
    {
        if (!parameters.AnyExtendedFilter())
            return null; // Log ids could get only if some extended filter was set.

        using var context = DbFactory.Create();

        var query = context.CreateQuery(parameters, true)
            .Select(o => new AuditLogItem() { Id = o.Id, Type = o.Type, Data = o.Data });

        var data = await query.ToListAsync(cancellationToken);
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
            () => IsValidFilter(item, AuditLogItemType.API, parameters.ApiRequestFilter)
        };

        return conditions.Any(o => o());
    }

    private static bool IsValidFilter(AuditLogItem item, AuditLogItemType type, IExtendedFilter filter)
    {
        if (item.Type != type) return false; // Invalid type.
        if (filter?.IsSet() != true) return true; // Filter not set.

        return filter.IsValid(item, JsonSerializerSettings);
    }

    public async Task<PaginatedResponse<AuditLogListItem>> GetListAsync(AuditLogListParams parameters, CancellationToken cancellationToken = default)
    {
        var logIds = await GetLogIdsAsync(parameters, cancellationToken);

        using var context = DbFactory.Create();

        var query = context.CreateQuery(parameters, true, true);
        if (logIds != null)
            query = query.Where(o => logIds.Contains(o.Id));

        return await PaginatedResponse<AuditLogListItem>
            .CreateAsync(query, parameters.Pagination, entity => MapItem(entity), cancellationToken);
    }

    private AuditLogListItem MapItem(AuditLogItem entity)
    {
        var mapped = Mapper.Map<AuditLogListItem>(entity);
        if (string.IsNullOrEmpty(entity.Data))
            return mapped;

        mapped.Data = entity.Type switch
        {
            AuditLogItemType.Error or AuditLogItemType.Info or AuditLogItemType.Warning => entity.Data,
            AuditLogItemType.Command => JsonConvert.DeserializeObject<CommandExecution>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.ChannelCreated or AuditLogItemType.ChannelDeleted => JsonConvert.DeserializeObject<AuditChannelInfo>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.ChannelUpdated => JsonConvert.DeserializeObject<Diff<AuditChannelInfo>>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.EmojiDeleted => JsonConvert.DeserializeObject<AuditEmoteInfo>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.GuildUpdated => JsonConvert.DeserializeObject<GuildUpdatedData>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.MemberRoleUpdated or AuditLogItemType.MemberUpdated => JsonConvert.DeserializeObject<MemberUpdatedData>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.MessageDeleted => JsonConvert.DeserializeObject<MessageDeletedData>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.MessageEdited => JsonConvert.DeserializeObject<MessageEditedData>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.OverwriteCreated or AuditLogItemType.OverwriteDeleted => JsonConvert.DeserializeObject<AuditOverwriteInfo>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.OverwriteUpdated => JsonConvert.DeserializeObject<Diff<AuditOverwriteInfo>>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.Unban => JsonConvert.DeserializeObject<AuditUserInfo>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.UserJoined => JsonConvert.DeserializeObject<UserJoinedAuditData>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.UserLeft => JsonConvert.DeserializeObject<UserLeftGuildData>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.InteractionCommand => JsonConvert.DeserializeObject<InteractionCommandExecuted>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.ThreadDeleted => JsonConvert.DeserializeObject<AuditThreadInfo>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.JobCompleted => JsonConvert.DeserializeObject<JobExecutionData>(entity.Data, JsonSerializerSettings),
            AuditLogItemType.API => JsonConvert.DeserializeObject<ApiRequest>(entity.Data, JsonSerializerSettings),
            _ => null
        };

        return mapped;
    }

    public async Task<FileInfo> GetLogItemFileAsync(long logId, long fileId, CancellationToken cancellationToken = default)
    {
        using var dbContext = DbFactory.Create();

        var logItem = await dbContext.AuditLogs.AsNoTracking()
            .Where(o => o.Id == logId)
            .Select(o => new { File = o.Files.FirstOrDefault(x => x.Id == fileId) })
            .FirstOrDefaultAsync(cancellationToken);

        if (logItem == null)
            throw new NotFoundException("Požadovaný záznam v logu nebyl nalezen.");

        if (logItem.File == null)
            throw new NotFoundException("K tomuto záznamu neexistuje žádný záznam o existenci souboru.");

        var storage = FileStorage.Create("Audit");
        var file = await storage.GetFileInfoAsync("DeletedAttachments", logItem.File.Filename);

        if (!file.Exists)
            throw new NotFoundException("Hledaný soubor neexistuje na disku.");

        return file;
    }

    public async Task<bool> RemoveItemAsync(long id)
    {
        using var context = DbFactory.Create();

        var item = await context.AuditLogs
            .Include(o => o.Files)
            .FirstOrDefaultAsync(o => o.Id == id);

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

            context.RemoveRange(item.Files);
        }

        context.Remove(item);
        return (await context.SaveChangesAsync()) > 0;
    }
}
