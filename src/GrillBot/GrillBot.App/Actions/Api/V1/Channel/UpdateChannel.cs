﻿using GrillBot.App.Managers;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class UpdateChannel : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private AutoReplyManager AutoReplyManager { get; }
    private AuditLogWriter AuditLogWriter { get; }
    private ITextsManager Texts { get; }
    private AuditLogService AuditLogService { get; }

    public UpdateChannel(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, AuditLogWriter auditLogWriter,
        ITextsManager texts, AuditLogService auditLogService, AutoReplyManager autoReplyManager) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        AuditLogWriter = auditLogWriter;
        Texts = texts;
        AuditLogService = auditLogService;
        AutoReplyManager = autoReplyManager;
    }

    public async Task ProcessAsync(ulong id, UpdateChannelParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var channel = await repository.Channel.FindChannelByIdAsync(id);
        if (channel == null)
            throw new NotFoundException(Texts["ChannelModule/ChannelDetail/ChannelNotFound", ApiContext.Language]);

        var before = new AuditChannelInfo(channel);
        var reloadAutoReply = channel.HasFlag(ChannelFlags.AutoReplyDeactivated) != ((parameters.Flags & (long)ChannelFlags.AutoReplyDeactivated) != 0);

        channel.Flags = parameters.Flags;

        var success = await repository.CommitAsync() > 0;
        if (!success) return;

        if (reloadAutoReply)
            await AutoReplyManager.InitAsync();

        var guild = await AuditLogService.GetGuildFromChannelAsync(null, id);
        var guildChannel = guild == null ? null : await guild.GetChannelAsync(id);
        var logItem = new AuditLogDataWrapper(AuditLogItemType.ChannelUpdated, new Diff<AuditChannelInfo>(before, new AuditChannelInfo(channel)), guild, guildChannel, ApiContext.LoggedUser);
        await AuditLogWriter.StoreAsync(logItem);
    }
}
