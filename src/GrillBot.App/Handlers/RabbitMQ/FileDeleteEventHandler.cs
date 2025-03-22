﻿using GrillBot.App.Helpers;
using GrillBot.Common.FileStorage;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.RabbitMQ.V2.Consumer;
using GrillBot.Core.Services.AuditLog.Models.Events;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ;

public class FileDeleteEventHandler : RabbitMessageHandlerBase<FileDeletePayload>
{
    private readonly BlobManagerFactoryHelper _blobManagerFactory;

    public FileDeleteEventHandler(ILoggerFactory loggerFactory, BlobManagerFactoryHelper blobManagerFactory) : base(loggerFactory)
    {
        _blobManagerFactory = blobManagerFactory;
    }

    protected override async Task<RabbitConsumptionResult> HandleInternalAsync(FileDeletePayload message, ICurrentUserProvider currentUser, Dictionary<string, string> headers)
    {
        var blobManager = await _blobManagerFactory.CreateAsync(BlobConstants.AuditLogDeletedAttachments);
        var legacyManager = await _blobManagerFactory.CreateLegacyAsync();

        await legacyManager.DeleteAsync(message.Filename);
        await blobManager.DeleteAsync(message.Filename);
        return RabbitConsumptionResult.Success;
    }
}
