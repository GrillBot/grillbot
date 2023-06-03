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
    private IFileServiceClient FileServiceClient { get; }

    public RemoveItem(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, IFileServiceClient fileServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
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

        foreach (var file in logItem.Files)
            await FileServiceClient.DeleteFileAsync(file.Filename);
        repository.RemoveCollection(logItem.Files);
    }
}
