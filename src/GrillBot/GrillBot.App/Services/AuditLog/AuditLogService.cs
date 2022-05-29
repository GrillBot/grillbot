using Discord.Commands;
using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog.Events;
using GrillBot.App.Services.FileStorage;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog;

[Initializable]
public partial class AuditLogService : ServiceBase
{
    public static JsonSerializerSettings JsonSerializerSettings { get; }
    private MessageCacheManager MessageCache { get; }
    private FileStorageFactory FileStorageFactory { get; }
    private InitManager InitManager { get; }

    private Dictionary<ulong, DateTime> NextAllowedChannelUpdateEvent { get; } = new();
    private DateTime NextAllowedRoleUpdateEvent { get; set; }

    static AuditLogService()
    {
        JsonSerializerSettings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore
        };
    }

    public AuditLogService(DiscordSocketClient client, GrillBotDatabaseFactory dbFactory, MessageCacheManager messageCache, FileStorageFactory storageFactory,
        InitManager initManager) : base(client, dbFactory)
    {
        MessageCache = messageCache;
        FileStorageFactory = storageFactory;
        InitManager = initManager;

        DiscordClient.UserLeft += (guild, user) => HandleEventAsync(new UserLeftEvent(this, guild, user));
        DiscordClient.UserJoined += user => HandleEventAsync(new UserJoinedEvent(this, user));
        DiscordClient.MessageUpdated += (before, after, channel) => HandleEventAsync(new MessageEditedEvent(this, before, after, channel, MessageCache, DiscordClient));
        DiscordClient.MessageDeleted += (message, channel) => HandleEventAsync(new MessageDeletedEvent(this, message, channel, MessageCache, FileStorageFactory));

        DiscordClient.ChannelCreated += channel => HandleEventAsync(new ChannelCreatedEvent(this, channel));
        DiscordClient.ChannelDestroyed += channel => HandleEventAsync(new ChannelDeletedEvent(this, channel));
        DiscordClient.ChannelUpdated += (before, after) => HandleEventAsync(new ChannelUpdatedEvent(this, before, after));
        DiscordClient.ChannelUpdated += async (_, after) =>
        {
            var nextAllowedEvent = NextAllowedChannelUpdateEvent.TryGetValue(after.Id, out var at) ? at : DateTime.MinValue;

            await HandleEventAsync(new OverwriteChangedEvent(this, after, nextAllowedEvent));
            nextAllowedEvent = DateTime.Now.AddMinutes(1);
            NextAllowedChannelUpdateEvent[after.Id] = nextAllowedEvent;
        };
        DiscordClient.GuildUpdated += (before, after) => HandleEventAsync(new EmotesUpdatedEvent(this, before, after));
        DiscordClient.GuildUpdated += (before, after) => HandleEventAsync(new GuildUpdatedEvent(this, before, after));

        DiscordClient.UserUnbanned += (user, guild) => HandleEventAsync(new UserUnbannedEvent(this, guild, user));
        DiscordClient.GuildMemberUpdated += (before, after) => HandleEventAsync(new MemberUpdatedEvent(this, before, after));
        DiscordClient.GuildMemberUpdated += async (before, after) =>
        {
            var @event = new MemberRolesUpdatedEvent(this, before, after, NextAllowedRoleUpdateEvent);
            await HandleEventAsync(@event);
            if (@event.Finished) NextAllowedRoleUpdateEvent = DateTime.Now.AddSeconds(30);
        };
        DiscordClient.ThreadDeleted += thread => HandleEventAsync(new ThreadDeletedEvent(this, thread));
    }

    /// <summary>
    /// Tries find guild from channel. If channel is DM method will return null;
    /// If channel is null and channelId is filled (typical usage for <see cref="Cacheable{TEntity, TId}"/>) method tries find guild with database data.
    /// </summary>
    public async Task<IGuild> GetGuildFromChannelAsync(IChannel channel, ulong channelId)
    {
        if (channel is IDMChannel) return null; // Direct messages
        if (channel is IGuildChannel guildChannel) return guildChannel.Guild;
        if (channel == null && channelId == default) return null;

        using var dbContext = DbFactory.Create();

        var guildId = await dbContext.Channels
            .Where(o => o.ChannelId == channelId.ToString())
            .Select(o => o.GuildId)
            .FirstOrDefaultAsync();

        return string.IsNullOrEmpty(guildId) ? null : DiscordClient.GetGuild(guildId.ToUlong());
    }

    public Task StoreItemAsync(AuditLogDataWrapper item, CancellationToken cancellationToken = default)
        => StoreItemsAsync(new() { item }, cancellationToken);

    public async Task StoreItemsAsync(List<AuditLogDataWrapper> items, CancellationToken cancellationToken = default)
    {
        using var dbContext = DbFactory.Create();

        foreach (var item in items.Where(o => o.Guild != null).DistinctBy(o => o.Guild.Id))
            await dbContext.InitGuildAsync(item.Guild, cancellationToken);

        foreach (var item in items.Where(o => o.ProcessedUser != null).DistinctBy(o => o.ProcessedUser.Id))
            await dbContext.InitUserAsync(item.ProcessedUser, cancellationToken);

        foreach (var item in items.Where(o => o.Guild != null && o.Channel != null).DistinctBy(o => o.Channel.Id))
            await dbContext.InitGuildChannelAsync(item.Guild, item.Channel, item.ChannelType.Value, cancellationToken);

        foreach (var item in items.Where(o => o.Guild != null && o.ProcessedUser != null).DistinctBy(o => o.ProcessedUser.Id))
        {
            var guildUser = item.ProcessedUser is not IGuildUser _guildUser ? await item.Guild.GetUserAsync(item.ProcessedUser.Id) : _guildUser;

            if (guildUser != null)
                await dbContext.InitGuildUserAsync(item.Guild, guildUser, cancellationToken);
        }

        await dbContext.AddRangeAsync(items.Select(o => o.ToEntity(JsonSerializerSettings)), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> CanExecuteEvent(Func<Task<bool>> eventSpecificCheck = null)
    {
        if (!InitManager.Get()) return false;
        if (eventSpecificCheck == null) return true;

        return await eventSpecificCheck();
    }

    private async Task HandleEventAsync(AuditEventBase @event)
    {
        if (await CanExecuteEvent(@event.CanProcessAsync))
            await @event.ProcessAsync();
    }

    public Task LogExecutedCommandAsync(CommandInfo command, ICommandContext context, global::Discord.Commands.IResult result, int duration)
        => HandleEventAsync(new ExecutedCommandEvent(this, command, context, result, duration));

    public Task LogExecutedInteractionCommandAsync(ICommandInfo command, IInteractionContext context, global::Discord.Interactions.IResult result,
        int duration)
    {
        return HandleEventAsync(new ExecutedInteractionCommandEvent(this, command, context, result, duration));
    }

    /// <summary>
    /// Gets IDs of audit log in discord.
    /// </summary>
    public async Task<List<ulong>> GetDiscordAuditLogIdsAsync(IGuild guild, IChannel channel, AuditLogItemType[] types, DateTime after)
    {
        using var dbContext = DbFactory.Create();

        var baseQuery = dbContext.AuditLogs.AsNoTracking()
            .Where(o => o.DiscordAuditLogItemId != null && o.CreatedAt >= after);

        if (guild != null)
            baseQuery = baseQuery.Where(o => o.GuildId == guild.Id.ToString());

        if (channel != null)
            baseQuery = baseQuery.Where(o => o.ChannelId == channel.Id.ToString());

        if (types?.Length > 0)
            baseQuery = baseQuery.Where(o => types.Contains(o.Type));

        var idsQuery = baseQuery.Select(o => o.DiscordAuditLogItemId).AsQueryable();
        var ids = await idsQuery.ToListAsync();
        return ids
            .SelectMany(o => o.Split(','))
            .Select(o => o.ToUlong())
            .Distinct()
            .ToList();
    }
}
