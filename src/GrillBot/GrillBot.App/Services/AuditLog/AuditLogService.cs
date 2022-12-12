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

    public AuditLogService(DiscordSocketClient client, GrillBotDatabaseBuilder databaseBuilder, InitManager initManager, AuditLogWriter auditLogWriter)
    {
        InitManager = initManager;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;
        AuditLogWriter = auditLogWriter;

        DiscordClient.ChannelCreated += channel => HandleEventAsync(new ChannelCreatedEvent(this, AuditLogWriter, channel));
    }

    /// <summary>
    /// Tries find guild from channel. If channel is DM method will return null;
    /// If channel is null and channelId is filled (typical usage for <see cref="Cacheable{TEntity, TId}"/>) method tries find guild with database data.
    /// </summary>
    [Obsolete("Use ChannelHelper.GetGuildFromChannelAsync")]
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

        var channelEntity = await repository.Channel.FindChannelByIdAsync(channelId, null, true, includeDeleted: true);
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
