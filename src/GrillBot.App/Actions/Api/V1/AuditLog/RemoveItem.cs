using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.FileService;
using GrillBot.Core.Exceptions;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class RemoveItem : ApiAction
{
    private ITextsManager Texts { get; }
    private IFileServiceClient FileServiceClient { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public RemoveItem(ApiRequestContext apiContext, ITextsManager texts, IFileServiceClient fileServiceClient, IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        Texts = texts;
        FileServiceClient = fileServiceClient;
        AuditLogServiceClient = auditLogServiceClient;
    }

    public async Task ProcessAsync(Guid id)
    {
        var response = await AuditLogServiceClient.DeleteItemAsync(id);
        if (!response.Exists)
            throw new NotFoundException(Texts["AuditLog/RemoveItem/NotFound", ApiContext.Language]);

        foreach (var file in response.FilesToDelete)
            await FileServiceClient.DeleteFileAsync(file);
    }
}
