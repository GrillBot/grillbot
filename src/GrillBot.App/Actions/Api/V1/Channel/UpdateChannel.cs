using GrillBot.App.Helpers;
using GrillBot.App.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class UpdateChannel : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private AutoReplyManager AutoReplyManager { get; }
    private ITextsManager Texts { get; }
    private ChannelHelper ChannelHelper { get; }
    private PointsHelper PointsHelper { get; }
    private IDiscordClient DiscordClient { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public UpdateChannel(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, AutoReplyManager autoReplyManager, ChannelHelper channelHelper,
        PointsHelper pointsHelper, IDiscordClient discordClient, IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        AutoReplyManager = autoReplyManager;
        ChannelHelper = channelHelper;
        PointsHelper = pointsHelper;
        DiscordClient = discordClient;
        AuditLogServiceClient = auditLogServiceClient;
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

        await WriteToAuditLogAsync(id, before, channel);
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

    private async Task WriteToAuditLogAsync(ulong channelId, Database.Entity.GuildChannel before, Database.Entity.GuildChannel after)
    {
        if (before.Flags == after.Flags)
            return;

        var guild = await ChannelHelper.GetGuildFromChannelAsync(null, channelId);
        var logRequest = new LogRequest
        {
            ChannelId = channelId.ToString(),
            ChannelUpdated = new DiffRequest<ChannelInfoRequest>
            {
                After = new ChannelInfoRequest { Flags = after.Flags },
                Before = new ChannelInfoRequest { Flags = before.Flags }
            },
            GuildId = guild?.Id.ToString(),
            Type = LogType.ChannelUpdated,
            CreatedAt = DateTime.UtcNow,
            UserId = ApiContext.GetUserId().ToString()
        };

        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
    }
}
