using GrillBot.App.Helpers;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Services.AuditLog;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class RemoveItem : ApiAction
{
    private ITextsManager Texts { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }
    private BlobManagerFactoryHelper BlobManagerFactoryHelper { get; }

    public RemoveItem(ApiRequestContext apiContext, ITextsManager texts, IAuditLogServiceClient auditLogServiceClient, BlobManagerFactoryHelper blobManagerFactoryHelper) : base(apiContext)
    {
        Texts = texts;
        AuditLogServiceClient = auditLogServiceClient;
        BlobManagerFactoryHelper = blobManagerFactoryHelper;
    }

    public async Task ProcessAsync(Guid id)
    {
        var response = await AuditLogServiceClient.DeleteItemAsync(id);
        if (!response.Exists)
            throw new NotFoundException(Texts["AuditLog/RemoveItem/NotFound", ApiContext.Language]);

        if (response.FilesToDelete.Count == 0)
            return;

        var manager = await BlobManagerFactoryHelper.CreateAsync(BlobConstants.AuditLogDeletedAttachments);
        var legacyManager = await BlobManagerFactoryHelper.CreateAsync("production");

        foreach (var filename in response.FilesToDelete)
        {
            await manager.DeleteAsync(filename);
            await legacyManager.DeleteAsync(filename);
        }
    }
}
