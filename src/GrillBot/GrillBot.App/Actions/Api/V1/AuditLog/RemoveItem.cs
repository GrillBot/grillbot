using GrillBot.Common.FileStorage;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;
using Microsoft.AspNetCore.Http;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class RemoveItem : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private FileStorageFactory FileStorage { get; }

    public RemoveItem(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, FileStorageFactory fileStorage) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        FileStorage = fileStorage;
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
            if (!fileInfo.Exists) continue;

            fileInfo.Delete();
        }

        repository.RemoveCollection(logItem.Files);
    }
}
