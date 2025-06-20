using GrillBot.App.Helpers;
using GrillBot.App.Managers;
using GrillBot.App.Managers.Points;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using GrillBot.Core.Services.MessageService.Models.Events;
using GrillBot.Core.Services.MessageService.Models.Events.Channels;
using GrillBot.Core.Services.PointsService.Models.Channels;
using GrillBot.Core.Services.PointsService.Models.Users;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class UpdateChannel : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private ChannelHelper ChannelHelper { get; }
    private IDiscordClient DiscordClient { get; }

    private readonly PointsManager _pointsManager;
    private readonly IRabbitPublisher _rabbitPublisher;

    public UpdateChannel(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, ChannelHelper channelHelper,
        IDiscordClient discordClient, PointsManager pointsManager, IRabbitPublisher rabbitPublisher) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        ChannelHelper = channelHelper;
        DiscordClient = discordClient;
        _pointsManager = pointsManager;
        _rabbitPublisher = rabbitPublisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var id = (ulong)Parameters[0]!;
        var parameters = (UpdateChannelParams)Parameters[1]!;

        using var repository = DatabaseBuilder.CreateRepository();

        var channel = await repository.Channel.FindChannelByIdAsync(id)
            ?? throw new NotFoundException(Texts["ChannelModule/ChannelDetail/ChannelNotFound", ApiContext.Language]);

        var before = channel.Clone();
        channel.Flags = parameters.Flags;

        var success = await repository.CommitAsync() > 0;
        if (!success)
            return ApiResult.Ok();

        await WriteToAuditLogAsync(id, before, channel);
        await SyncPointsServiceAsync(channel);
        await SynchronizeMessageServiceAsync(before, channel);

        return ApiResult.Ok();
    }

    private async Task SyncPointsServiceAsync(Database.Entity.GuildChannel after)
    {
        var guild = await DiscordClient.GetGuildAsync(after.GuildId.ToUlong());
        if (guild is null) return;

        var channel = await guild.GetChannelAsync(after.ChannelId.ToUlong());
        if (channel is null) return;

        var syncItem = new ChannelSyncItem
        {
            Id = after.ChannelId,
            IsDeleted = after.HasFlag(ChannelFlag.Deleted),
            PointsDisabled = after.HasFlag(ChannelFlag.PointsDeactivated)
        };

        await _pointsManager.PushSynchronizationAsync(guild, Enumerable.Empty<UserSyncItem>(), new[] { syncItem });
    }

    private async Task WriteToAuditLogAsync(ulong channelId, Database.Entity.GuildChannel before, Database.Entity.GuildChannel after)
    {
        if (before.Flags == after.Flags)
            return;

        var guild = await ChannelHelper.GetGuildFromChannelAsync(null, channelId);
        var userId = ApiContext.GetUserId().ToString();
        var logRequest = new LogRequest(LogType.ChannelUpdated, DateTime.UtcNow, guild?.Id.ToString(), userId, channelId.ToString())
        {
            ChannelUpdated = new DiffRequest<ChannelInfoRequest>
            {
                After = new ChannelInfoRequest { Flags = (int)after.Flags },
                Before = new ChannelInfoRequest { Flags = (int)before.Flags }
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsMessage(logRequest));
    }

    private Task SynchronizeMessageServiceAsync(Database.Entity.GuildChannel before, Database.Entity.GuildChannel after)
    {
        if (
            before.HasFlag(ChannelFlag.PointsDeactivated) == after.HasFlag(ChannelFlag.PointsDeactivated) &&
            before.HasFlag(ChannelFlag.AutoReplyDeactivated) == after.HasFlag(ChannelFlag.AutoReplyDeactivated)
        )
        {
            // No change in flags that are synchronized to MessageService.
            return Task.CompletedTask;
        }

        var syncItem = new ChannelSynchronizationItem(after.GuildId.ToUlong(), after.ChannelId.ToUlong())
        {
            IsAutoReplyDisabled = after.HasFlag(ChannelFlag.AutoReplyDeactivated),
            IsPointsDisabled = after.HasFlag(ChannelFlag.PointsDeactivated),
            IsDeleted = after.HasFlag(ChannelFlag.Deleted)
        };

        return _rabbitPublisher.PublishAsync(new SynchronizationPayload([syncItem]));
    }
}
