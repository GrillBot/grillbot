using Discord.Commands;
using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog.Events;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog;

[Initializable]
public class AuditLogService
{
    private InitManager InitManager { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private AuditLogWriter AuditLogWriter { get; }
    private IServiceProvider ServiceProvider { get; }

    private Dictionary<ulong, DateTime> NextAllowedChannelUpdateEvent { get; } = new();
    private DateTime NextAllowedRoleUpdateEvent { get; set; }

    public AuditLogService(DiscordSocketClient client, GrillBotDatabaseBuilder databaseBuilder, InitManager initManager, AuditLogWriter auditLogWriter, IServiceProvider serviceProvider)
    {
        InitManager = initManager;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;
        AuditLogWriter = auditLogWriter;
        ServiceProvider = serviceProvider;

        DiscordClient.UserLeft += (guild, user) => HandleEventAsync(new UserLeftEvent(this, AuditLogWriter, guild, user));
        DiscordClient.UserJoined += user => HandleEventAsync(new UserJoinedEvent(this, AuditLogWriter, user));
        DiscordClient.MessageUpdated += (before, after, channel) => HandleEventAsync(new MessageEditedEvent(this, AuditLogWriter, ServiceProvider, before, after, channel));
        DiscordClient.MessageDeleted += (message, channel) => HandleEventAsync(new MessageDeletedEvent(this, AuditLogWriter, ServiceProvider, message, channel));
        DiscordClient.ChannelCreated += channel => HandleEventAsync(new ChannelCreatedEvent(this, AuditLogWriter, channel));
        DiscordClient.ChannelDestroyed += channel => HandleEventAsync(new ChannelDeletedEvent(this, AuditLogWriter, channel));
        DiscordClient.ChannelUpdated += (before, after) => HandleEventAsync(new ChannelUpdatedEvent(this, AuditLogWriter, before, after));
        DiscordClient.ChannelUpdated += async (_, after) =>
        {
            var nextAllowedEvent = NextAllowedChannelUpdateEvent.TryGetValue(after.Id, out var at) ? at : DateTime.MinValue;
            await HandleEventAsync(new OverwriteChangedEvent(this, AuditLogWriter, after, nextAllowedEvent));
            nextAllowedEvent = DateTime.Now.AddMinutes(1);
            NextAllowedChannelUpdateEvent[after.Id] = nextAllowedEvent;
        };
        DiscordClient.GuildUpdated += (before, after) => HandleEventAsync(new EmotesUpdatedEvent(this, AuditLogWriter, before, after));
        DiscordClient.GuildUpdated += (before, after) => HandleEventAsync(new GuildUpdatedEvent(this, AuditLogWriter, before, after));
        DiscordClient.UserUnbanned += (user, guild) => HandleEventAsync(new UserUnbannedEvent(this, AuditLogWriter, guild, user));
        DiscordClient.GuildMemberUpdated += (before, after) => HandleEventAsync(new MemberUpdatedEvent(this, AuditLogWriter, before, after));
        DiscordClient.GuildMemberUpdated += async (before, after) =>
        {
            var @event = new MemberRolesUpdatedEvent(this, AuditLogWriter, before, after, NextAllowedRoleUpdateEvent);
            await HandleEventAsync(@event);
            if (@event.Finished) NextAllowedRoleUpdateEvent = DateTime.Now.AddSeconds(30);
        };
        DiscordClient.ThreadDeleted += thread => HandleEventAsync(new ThreadDeletedEvent(this, AuditLogWriter, thread));
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

        await using var repository = DatabaseBuilder.CreateRepository();

        var channelEntity = await repository.Channel.FindChannelByIdAsync(channelId, null, true);
        if (channelEntity == null)
            return null;

        var guildId = channelEntity.GuildId.ToUlong();
        return DiscordClient.GetGuild(guildId);
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
        => HandleEventAsync(new ExecutedCommandEvent(this, AuditLogWriter, command, context, result, duration));

    public Task LogExecutedInteractionCommandAsync(ICommandInfo command, IInteractionContext context, global::Discord.Interactions.IResult result,
        int duration)
    {
        return HandleEventAsync(new ExecutedInteractionCommandEvent(this, AuditLogWriter, command, context, result, duration));
    }

    /// <summary>
    /// Gets IDs of audit log in discord.
    /// </summary>
    public async Task<List<ulong>> GetDiscordAuditLogIdsAsync(IGuild guild, IChannel channel, AuditLogItemType[] types, DateTime after)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.AuditLog.GetDiscordAuditLogIdsAsync(guild, channel, types, after);
    }
}
