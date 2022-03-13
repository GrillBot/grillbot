using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.Common;
using Microsoft.AspNetCore.StaticFiles;

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
        return await PaginatedResponse<AuditLogListItem>.CreateAsync(query, parameters, entity => new(entity, JsonSerializerSettings), cancellationToken);
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
