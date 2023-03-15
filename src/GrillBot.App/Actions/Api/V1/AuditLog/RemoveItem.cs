using GrillBot.Common.FileStorage;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Common.Services.FileService;
using GrillBot.Core.Exceptions;
using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class RemoveItem : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private FileStorageFactory FileStorage { get; }
    private IFileServiceClient FileServiceClient { get; }

    public RemoveItem(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, FileStorageFactory fileStorage,
        IFileServiceClient fileServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        FileStorage = fileStorage;
        FileServiceClient = fileServiceClient;
    }

    public async Task ProcessAsync(long id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var logItem = await repository.AuditLog.FindLogItemByIdAsync(id, true);
        if (logItem == null)
            throw new NotFoundException(Texts["AuditLog/RemoveItem/NotFound", ApiContext.Language]);

        await RemoveFilesAsync(repository, logItem);
        repository.Remove(logItem);

        await repository.CommitAsync();
    }

    private async Task RemoveFilesAsync(GrillBotRepository repository, AuditLogItem logItem)
    {
        if (logItem.Files.Count == 0) return;

        var storage = FileStorage.Create("Audit");
        foreach (var file in logItem.Files)
        {
            var fileInfo = await storage.GetFileInfoAsync("DeletedAttachments", file.Filename);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
                continue;
            }

            await FileServiceClient.DeleteFileAsync(file.Filename);
        }

        repository.RemoveCollection(logItem.Files);
    }
}
