using Discord.Commands;
using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog.Events;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog;

[Initializable]
public class AuditLogService : ServiceBase
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

    public AuditLogService(DiscordSocketClient client, GrillBotDatabaseBuilder dbFactory, MessageCacheManager messageCache, FileStorageFactory storageFactory,
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
        switch (channel)
        {
            case IDMChannel:
                return null; // Direct messages
            case IGuildChannel guildChannel:
                return guildChannel.Guild;
            case null when channelId == default:
                return null;
        }

        await using var repository = DbFactory.CreateRepository();

        var channels = await repository.Channel.FindChannelsByIdAsync(channelId, true);
        var channelEntity = channels.FirstOrDefault();
        if (channelEntity == null)
            return null;

        var guildId = channelEntity.GuildId.ToUlong();
        return DiscordClient.GetGuild(guildId);
    }

    public Task StoreItemAsync(AuditLogDataWrapper item)
        => StoreItemsAsync(new List<AuditLogDataWrapper> { item });

    public async Task StoreItemsAsync(List<AuditLogDataWrapper> items)
    {
        await using var repository = DbFactory.CreateRepository();

        foreach (var item in items.Where(o => o.Guild != null).DistinctBy(o => o.Guild.Id))
            await repository.Guild.GetOrCreateRepositoryAsync(item.Guild);

        foreach (var item in items.Where(o => o.Guild != null && o.Channel != null).DistinctBy(o => o.Channel.Id))
            await repository.Channel.GetOrCreateChannelAsync(item.Channel);

        foreach (var item in items.Where(o => o.ProcessedUser != null).DistinctBy(o => o.ProcessedUser.Id))
        {
            if (item.Guild != null)
            {
                var guildUser = item.ProcessedUser as IGuildUser ?? await item.Guild.GetUserAsync(item.ProcessedUser.Id);
                if (guildUser != null)
                    await repository.GuildUser.GetOrCreateGuildUserAsync(guildUser);
            }
            else
            {
                await repository.User.GetOrCreateUserAsync(item.ProcessedUser);
            }
        }

        await repository.AddRangeAsync(items.Select(o => o.ToEntity(JsonSerializerSettings)));
        await repository.CommitAsync();
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
        await using var repository = DbFactory.CreateRepository();
        return await repository.AuditLog.GetDiscordAuditLogIdsAsync(guild, channel, types, after);
    }
}
