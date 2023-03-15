using GrillBot.Common.FileStorage;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using Microsoft.AspNetCore.StaticFiles;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class GetFileContent : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private FileExtensionContentTypeProvider ContentTypeProvider { get; }
    private FileStorageFactory FileStorage { get; }
    private ITextsManager Texts { get; }

    public GetFileContent(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, FileStorageFactory fileStorage,
        ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        FileStorage = fileStorage;
        ContentTypeProvider = new FileExtensionContentTypeProvider();
        Texts = texts;
    }

    public async Task<(byte[] content, string contentType)> ProcessAsync(long logId, long fileId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var errMsg = Texts["AuditLog/GetFileContent/NotFound", ApiContext.Language];
        var logItem = await repository.AuditLog.FindLogItemByIdAsync(logId, true);
        var metadata = logItem?.Files.FirstOrDefault(o => o.Id == fileId);
        if (metadata == null) throw new NotFoundException(errMsg);

        var storage = FileStorage.Create("Audit");
        var file = await storage.GetFileInfoAsync("DeletedAttachments", metadata.Filename);
        if (!file.Exists) throw new NotFoundException(errMsg);

        if (!ContentTypeProvider.TryGetContentType(file.FullName, out var contentType))
            contentType = "application/octet-stream";

        var content = await File.ReadAllBytesAsync(file.FullName);
        return (content, contentType);
    }
}
