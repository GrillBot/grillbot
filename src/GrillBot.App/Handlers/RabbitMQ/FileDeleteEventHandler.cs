using GrillBot.App.Helpers;
using GrillBot.Common.FileStorage;
using GrillBot.Core.RabbitMQ.Consumer;
using GrillBot.Core.Services.AuditLog.Models.Events;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ;

public class FileDeleteEventHandler : BaseRabbitMQHandler<FileDeletePayload>
{
    public override string QueueName => new FileDeletePayload().QueueName;

    private readonly BlobManagerFactoryHelper _blobManagerFactory;

    public FileDeleteEventHandler(ILoggerFactory loggerFactory, BlobManagerFactoryHelper blobManagerFactory) : base(loggerFactory)
    {
        _blobManagerFactory = blobManagerFactory;
    }

    protected override async Task HandleInternalAsync(FileDeletePayload payload, Dictionary<string, string> headers)
    {
        var blobManager = await _blobManagerFactory.CreateAsync(BlobConstants.AuditLogDeletedAttachments);
        var legacyManager = await _blobManagerFactory.CreateLegacyAsync();

        await legacyManager.DeleteAsync(payload.Filename);
        await blobManager.DeleteAsync(payload.Filename);
    }
}
