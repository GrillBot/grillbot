using GrillBot.App.Helpers;
using GrillBot.App.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public partial class AuditOrchestrationHandler : IGuildMemberUpdatedEvent, IGuildUpdatedEvent, IChannelCreatedEvent, IChannelDestroyedEvent, IChannelUpdatedEvent, IMessageUpdatedEvent,
    IRoleDeletedEvent, IThreadDeletedEvent, IUserJoinedEvent, IUserLeftEvent, IUserUnbannedEvent
{
    private readonly IRabbitPublisher _rabbitPublisher;
    private readonly AuditLogManager _auditLogManager;
    private readonly ICounterManager _counterManager;
    private readonly DownloadHelper _downloadHelper;
    private readonly IMessageCacheManager _messageCache;
    private readonly ChannelHelper _channelHelper;

    public AuditOrchestrationHandler(IRabbitPublisher rabbitPublisher, AuditLogManager auditLogManager, ICounterManager counterManager, DownloadHelper downloadHelper,
        IMessageCacheManager messageCache, ChannelHelper channelHelper)
    {
        _rabbitPublisher = rabbitPublisher;
        _counterManager = counterManager;
        _auditLogManager = auditLogManager;
        _downloadHelper = downloadHelper;
        _messageCache = messageCache;
        _channelHelper = channelHelper;
    }


    // GuildMemberUpdated
    public async Task ProcessAsync(IGuildUser? before, IGuildUser after)
    {
        var payload = new CreateItemsMessage();

        await ProcessRoleChangesAsync(before, after, payload);
        ProcessUserChanges(before, after, payload);

        await PushPayloadAsync(payload);
    }

    // GuildUpdated
    public async Task ProcessAsync(IGuild before, IGuild after)
    {
        var payload = new CreateItemsMessage();

        ProcessRemovedEmotes(before, after, payload);
        await ProcessGuildChangesAsync(before, after, payload);

        await PushPayloadAsync(payload);
    }

    // ChannelUpdated
    public async Task ProcessAsync(IChannel before, IChannel after)
    {
        var payload = new CreateItemsMessage();

        ProcessChannelChanges(before, after, payload);
        await ProcessOverwriteChangesAsync(after, payload);

        await PushPayloadAsync(payload);
    }

    // MessageUpdated
    public async Task ProcessAsync(Cacheable<IMessage, ulong> before, IMessage after, IMessageChannel channel)
    {
        var oldMessage = before.HasValue ? before.Value : null;
        oldMessage ??= await _messageCache.GetAsync(before.Id, null);

        if (channel is not ITextChannel textChannel) return;
        if (oldMessage?.Author.IsUser() != true || after.Author.Id == 0) return;
        if (oldMessage.Content == after.Content) return;
        if (oldMessage.Type is MessageType.ApplicationCommand or MessageType.ContextMenuCommand) return;

        var author = after.Author as IGuildUser ?? await textChannel.Guild.GetUserAsync(after.Author.Id);
        var guildId = textChannel.GuildId.ToString();
        var channelId = textChannel.Id.ToString();
        var logRequest = new LogRequest(LogType.MessageEdited, DateTime.UtcNow, guildId, author.Id.ToString(), channelId)
        {
            MessageEdited = new MessageEditedRequest
            {
                ContentAfter = after.Content,
                ContentBefore = oldMessage.Content,
                JumpUrl = after.GetJumpUrl()
            }
        };

        await PushPayloadAsync(logRequest);
    }

    // RoleDeleted
    public Task ProcessAsync(IRole role)
    {
        if (role.Guild is null) return Task.CompletedTask;

        var logRequest = new LogRequest(LogType.RoleDeleted, DateTime.UtcNow, role.Guild.Id.ToString())
        {
            RoleDeleted = new RoleDeletedRequest { RoleId = role.Id.ToString() }
        };

        return PushPayloadAsync(logRequest);
    }

    // ThreadDeleted
    public async Task ProcessAsync(IThreadChannel? cachedThread, ulong threadId)
    {
        var guild = await _channelHelper.GetGuildFromChannelAsync(cachedThread, threadId);
        if (guild is null) return;

        var logRequest = new LogRequest(LogType.ThreadDeleted, DateTime.UtcNow, guild.Id.ToString(), null, threadId.ToString())
        {
            ThreadInfo = new ThreadInfoRequest { Tags = new List<string>() }
        };

        await PushPayloadAsync(logRequest);
    }

    // UserJoined
    public Task ProcessAsync(IGuildUser user)
    {
        if (!user.IsUser()) return Task.CompletedTask;

        var guildId = user.GuildId.ToString();
        var userId = user.Id.ToString();
        var logRequest = new LogRequest(LogType.UserJoined, DateTime.UtcNow, guildId, userId)
        {
            UserJoined = new UserJoinedRequest { MemberCount = (int)user.Guild.GetMemberCount() }
        };

        return PushPayloadAsync(logRequest);
    }

    // UserLeft
    public async Task ProcessAsync(IGuild guild, IUser user)
    {
        var currentUser = await guild.GetCurrentUserAsync();
        if (user.Id == currentUser.Id) return;

        var logRequest = new LogRequest(LogType.UserLeft, DateTime.UtcNow, guild.Id.ToString(), user.Id.ToString())
        {
            UserLeft = new UserLeftRequest
            {
                MemberCount = (int)guild.GetMemberCount(),
                UserId = user.Id.ToString()
            }
        };

        await PushPayloadAsync(logRequest);
    }

    // UserUnbanned
    public Task ProcessAsync(IUser user, IGuild guild)
    {
        var guildId = guild.Id.ToString();
        var logRequest = new LogRequest(LogType.Unban, DateTime.UtcNow, guildId)
        {
            Unban = new UnbanRequest { UserId = user.Id.ToString() }
        };

        return PushPayloadAsync(logRequest);
    }

    // ChannelCreated
    Task IChannelCreatedEvent.ProcessAsync(IChannel channel)
    {
        if (channel is not IGuildChannel guildChannel) return Task.CompletedTask;

        var guildId = guildChannel.GuildId.ToString();
        var channelId = channel.Id.ToString();

        var logRequest = new LogRequest(LogType.ChannelCreated, DateTime.UtcNow, guildId, null, channelId)
        {
            ChannelInfo = new ChannelInfoRequest
            {
                Position = guildChannel.Position,
                Topic = (guildChannel as ITextChannel)?.Topic
            }
        };

        return PushPayloadAsync(logRequest);
    }

    // ChannelDestroyed
    Task IChannelDestroyedEvent.ProcessAsync(IChannel channel)
    {
        if (channel is not IGuildChannel guildChannel) return Task.CompletedTask;

        var guildId = guildChannel.GuildId.ToString();
        var channelId = guildChannel.Id.ToString();

        var logRequest = new LogRequest(LogType.ChannelDeleted, DateTime.UtcNow, guildId, null, channelId)
        {
            ChannelInfo = new ChannelInfoRequest
            {
                Position = guildChannel.Position,
                Topic = (guildChannel as ITextChannel)?.Topic
            }
        };

        return PushPayloadAsync(logRequest);
    }
}
