using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Models;

namespace GrillBot.App.Services.AuditLog;

public class AuditLogApiService
{
    private FileStorageFactory FileStorage { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ApiRequestContext ApiRequestContext { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public AuditLogApiService(GrillBotDatabaseBuilder databaseBuilder, FileStorageFactory fileStorage,
        ApiRequestContext apiRequestContext, AuditLogWriter auditLogWriter)
    {
        FileStorage = fileStorage;
        DatabaseBuilder = databaseBuilder;
        ApiRequestContext = apiRequestContext;
        AuditLogWriter = auditLogWriter;
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

    public async Task HandleClientAppMessageAsync(ClientLogItemRequest request)
    {
        var item = new AuditLogDataWrapper(request.GetAuditLogType(), request.Content, processedUser: ApiRequestContext.LoggedUser);
        await AuditLogWriter.StoreAsync(item);
    }
}
