using GrillBot.App.Helpers;
using GrillBot.App.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
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
    private PointsHelper PointsHelper { get; }
    private IDiscordClient DiscordClient { get; }

    public UpdateChannel(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, AuditLogWriteManager auditLogWriteManager, ITextsManager texts, AutoReplyManager autoReplyManager,
        ChannelHelper channelHelper, PointsHelper pointsHelper, IDiscordClient discordClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        AuditLogWriteManager = auditLogWriteManager;
        Texts = texts;
        AutoReplyManager = autoReplyManager;
        ChannelHelper = channelHelper;
        PointsHelper = pointsHelper;
        DiscordClient = discordClient;
    }

    public async Task ProcessAsync(ulong id, UpdateChannelParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var channel = await repository.Channel.FindChannelByIdAsync(id);
        if (channel == null)
            throw new NotFoundException(Texts["ChannelModule/ChannelDetail/ChannelNotFound", ApiContext.Language]);

        var before = channel.Clone();
        channel.Flags = parameters.Flags;

        var success = await repository.CommitAsync() > 0;
        if (!success) return;

        var guild = await ChannelHelper.GetGuildFromChannelAsync(null, id);
        var guildChannel = guild == null ? null : await guild.GetChannelAsync(id);
        var logItem = new AuditLogDataWrapper(AuditLogItemType.ChannelUpdated, new Diff<AuditChannelInfo>(new AuditChannelInfo(before), new AuditChannelInfo(channel)), guild, guildChannel,
            ApiContext.LoggedUser);
        await AuditLogWriteManager.StoreAsync(logItem);

        await TryReloadAutoReplyAsync(before, channel);
        await TrySyncPointsService(before, channel);
    }

    private async Task TryReloadAutoReplyAsync(Database.Entity.GuildChannel before, Database.Entity.GuildChannel after)
    {
        if (before.HasFlag(ChannelFlag.AutoReplyDeactivated) != after.HasFlag(ChannelFlag.AutoReplyDeactivated))
            await AutoReplyManager.InitAsync();
    }

    private async Task TrySyncPointsService(Database.Entity.GuildChannel before, Database.Entity.GuildChannel after)
    {
        if (before.HasFlag(ChannelFlag.PointsDeactivated) == after.HasFlag(ChannelFlag.PointsDeactivated))
            return;

        var guild = await DiscordClient.GetGuildAsync(after.GuildId.ToUlong());
        var channel = await guild.GetChannelAsync(after.ChannelId.ToUlong());
        if (channel is null) return;

        await PointsHelper.SyncDataWithServiceAsync(guild, Enumerable.Empty<IUser>(), new[] { channel });
    }
}
