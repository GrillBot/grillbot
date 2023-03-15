using GrillBot.App.Helpers;
using GrillBot.App.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Models;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class UpdateChannel : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private AutoReplyManager AutoReplyManager { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }
    private ITextsManager Texts { get; }
    private ChannelHelper ChannelHelper { get; }

    public UpdateChannel(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, AuditLogWriteManager auditLogWriteManager, ITextsManager texts, AutoReplyManager autoReplyManager,
        ChannelHelper channelHelper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        AuditLogWriteManager = auditLogWriteManager;
        Texts = texts;
        AutoReplyManager = autoReplyManager;
        ChannelHelper = channelHelper;
    }

    public async Task ProcessAsync(ulong id, UpdateChannelParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var channel = await repository.Channel.FindChannelByIdAsync(id);
        if (channel == null)
            throw new NotFoundException(Texts["ChannelModule/ChannelDetail/ChannelNotFound", ApiContext.Language]);

        var before = new AuditChannelInfo(channel);
        var reloadAutoReply = channel.HasFlag(ChannelFlag.AutoReplyDeactivated) != ((parameters.Flags & (long)ChannelFlag.AutoReplyDeactivated) != 0);

        channel.Flags = parameters.Flags;

        var success = await repository.CommitAsync() > 0;
        if (!success) return;

        if (reloadAutoReply)
            await AutoReplyManager.InitAsync();

        var guild = await ChannelHelper.GetGuildFromChannelAsync(null, id);
        var guildChannel = guild == null ? null : await guild.GetChannelAsync(id);
        var logItem = new AuditLogDataWrapper(AuditLogItemType.ChannelUpdated, new Diff<AuditChannelInfo>(before, new AuditChannelInfo(channel)), guild, guildChannel, ApiContext.LoggedUser);
        await AuditLogWriteManager.StoreAsync(logItem);
    }
}
