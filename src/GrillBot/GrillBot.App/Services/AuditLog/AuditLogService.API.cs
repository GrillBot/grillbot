using GrillBot.Data.Exceptions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog;

public partial class AuditLogService
{
    public async Task<PaginatedResponse<AuditLogListItem>> GetPaginatedListAsync(AuditLogListParams parameters, CancellationToken cancellationToken = default)
    {
        using var dbContext = DbFactory.Create();

        var query = dbContext.AuditLogs.AsNoTracking()
            .Include(o => o.Files)
            .Include(o => o.Guild)
            .Include(o => o.GuildChannel)
            .Include(o => o.ProcessedGuildUser).ThenInclude(o => o.User)
            .Include(o => o.ProcessedUser)
            .AsSplitQuery().AsQueryable();

        query = parameters.CreateQuery(query);
        return await PaginatedResponse<AuditLogListItem>.CreateAsync(query, parameters, entity => MapItem(entity), cancellationToken);
    }

    private AuditLogListItem MapItem(AuditLogItem entity)
    {
        var mapped = Mapper.Map<AuditLogListItem>(entity);

        if (!string.IsNullOrEmpty(entity.Data))
        {
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
                _ => null
            };
        }

        return mapped;
    }

    public async Task<FileInfo> GetLogItemFileAsync(long logId, long fileId, CancellationToken cancellationToken = default)
    {
        using var dbContext = DbFactory.Create();

        var logItem = await dbContext.AuditLogs.AsNoTracking()
            .Where(o => o.Id == logId)
            .Select(o => new
            {
                File = o.Files.FirstOrDefault(x => x.Id == fileId)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (logItem == null)
            throw new NotFoundException("Požadovaný záznam v logu nebyl nalezen.");

        if (logItem.File == null)
            throw new NotFoundException("K tomuto záznamu neexistuje žádný záznam o existenci souboru.");

        var storage = FileStorageFactory.Create("Audit");
        var file = await storage.GetFileInfoAsync("DeletedAttachments", logItem.File.Filename);

        if (!file.Exists)
            throw new NotFoundException("Hledaný soubor neexistuje na disku.");

        return file;
    }
}
