using Discord.Commands;
using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog.Events;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.FileStorage;
using GrillBot.Data.Helpers;
using GrillBot.Data.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog;

public partial class AuditLogService : ServiceBase
{
    public static JsonSerializerSettings JsonSerializerSettings { get; }
    private MessageCache.MessageCache MessageCache { get; }
    private FileStorageFactory FileStorageFactory { get; }

    private DateTime NextAllowedChannelUpdateEvent { get; set; }
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

    public AuditLogService(DiscordSocketClient client, GrillBotContextFactory dbFactory, MessageCache.MessageCache cache,
        FileStorageFactory storageFactory, DiscordInitializationService initializationService) : base(client, dbFactory, initializationService)
    {
        MessageCache = cache;

        DiscordClient.UserLeft += (guild, user) => HandleEventAsync(new UserLeftEvent(this, guild, user));
        DiscordClient.UserJoined += user => HandleEventAsync(new UserJoinedEvent(this, user));
        DiscordClient.MessageUpdated += (before, after, channel) => HandleEventAsync(new MessageEditedEvent(this, before, after, channel, MessageCache, DiscordClient));
        DiscordClient.MessageDeleted += (message, channel) => HandleEventAsync(new MessageDeletedEvent(this, message, channel, MessageCache, FileStorageFactory));

        DiscordClient.ChannelCreated += channel => HandleEventAsync(new ChannelCreatedEvent(this, channel));
        DiscordClient.ChannelDestroyed += channel => HandleEventAsync(new ChannelDeletedEvent(this, channel));
        DiscordClient.ChannelUpdated += (before, after) => HandleEventAsync(new ChannelUpdatedEvent(this, before, after));
        DiscordClient.ChannelUpdated += async (_, after) =>
        {
            await HandleEventAsync(new OverwriteChangedEvent(this, after, NextAllowedChannelUpdateEvent, DbFactory));
            NextAllowedChannelUpdateEvent = DateTime.Now.AddMinutes(1);
        };
        DiscordClient.GuildUpdated += (before, after) => HandleEventAsync(new EmotesUpdatedEvent(this, before, after));
        DiscordClient.GuildUpdated += (before, after) => HandleEventAsync(new GuildUpdatedEvent(this, before, after));

        DiscordClient.UserUnbanned += OnUserUnbannedAsync;
        DiscordClient.GuildMemberUpdated += (before, after) =>
        {
            if (!before.HasValue) return Task.CompletedTask;
            if (!InitializationService.Get()) return Task.CompletedTask;
            if (IsMemberReallyUpdated(before.Value, after))
                return OnMemberUpdatedAsync(before.Value, after);

            if (!before.Value.Roles.SequenceEqual(after.Roles) && NextAllowedRoleUpdateEvent <= DateTime.Now)
            {
                var task = OnMemberRolesUpdatedAsync(after);
                NextAllowedRoleUpdateEvent = DateTime.Now.AddSeconds(30);
                return task;
            }

            return Task.CompletedTask;
        };
        DiscordClient.ThreadDeleted += thread => HandleEventAsync(new ThreadDeletedEvent(this, thread));

        // TODO: Impelement audit log download after restart.

        FileStorageFactory = storageFactory;
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

        return string.IsNullOrEmpty(guildId) ? null : DiscordClient.GetGuild(Convert.ToUInt64(guildId));
    }

    /// <summary>
    /// Stores new item in log. Method will check relationships in database and create if some will be required.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="guild"></param>
    /// <param name="channel"></param>
    /// <param name="processedUser"></param>
    /// <param name="data"></param>
    /// <param name="auditLogItemId">ID of discord audit log record. Allowed types are ulong?, string or null. Otherwise method throws <see cref="NotSupportedException"/></param>
    /// <param name="cancellationToken"></param>
    /// <param name="attachments"></param>
    public async Task StoreItemAsync(AuditLogItemType type, IGuild guild, IChannel channel, IUser processedUser, string data, object auditLogItemId = null,
        CancellationToken? cancellationToken = null, List<AuditLogFileMeta> attachments = null)
    {
        AuditLogItem entity;
        if (auditLogItemId is null)
            entity = AuditLogItem.Create(type, guild, channel, processedUser, data);
        else if (auditLogItemId is string _auditLogItemId)
            entity = AuditLogItem.Create(type, guild, channel, processedUser, data, _auditLogItemId);
        else if (auditLogItemId is ulong __auditLogItemId)
            entity = AuditLogItem.Create(type, guild, channel, processedUser, data, __auditLogItemId);
        else
            throw new NotSupportedException("Unsupported type Discord audit log item ID.");

        attachments?.ForEach(a => entity.Files.Add(a));
        using var dbContext = DbFactory.Create();

        if (processedUser != null)
            await dbContext.InitUserAsync(processedUser, cancellationToken ?? CancellationToken.None);

        if (guild != null)
        {
            await dbContext.InitGuildAsync(guild, cancellationToken ?? CancellationToken.None);

            if (processedUser != null)
            {
                if (processedUser is not IGuildUser guildUser)
                    guildUser = await guild.GetUserAsync(processedUser.Id);

                if (guildUser != null)
                    await dbContext.InitGuildUserAsync(guild, guildUser, cancellationToken ?? CancellationToken.None);
            }

            if (channel != null)
            {
                var channelType = DiscordHelper.GetChannelType(channel);
                await dbContext.InitGuildChannelAsync(guild, channel, channelType.Value, cancellationToken ?? CancellationToken.None);
            }
        }

        await dbContext.AddAsync(entity, cancellationToken ?? CancellationToken.None);
        await dbContext.SaveChangesAsync(cancellationToken ?? CancellationToken.None);
    }

    private async Task<bool> CanExecuteEvent(Func<Task<bool>> eventSpecificCheck = null)
    {
        if (!InitializationService.Get()) return false;
        if (eventSpecificCheck == null) return true;

        return await eventSpecificCheck();
    }

    private async Task HandleEventAsync(AuditEventBase @event)
    {
        if (await CanExecuteEvent(@event.CanProcessAsync))
            await @event.ProcessAsync();
    }

    public async Task LogExecutedCommandAsync(CommandInfo command, ICommandContext context, global::Discord.Commands.IResult result)
    {
        var data = new CommandExecution(command, context.Message, result);
        var logItem = AuditLogItem.Create(AuditLogItemType.Command, context.Guild, context.Channel, context.Guild != null ? context.User : null, JsonConvert.SerializeObject(data, JsonSerializerSettings));

        using var dbContext = DbFactory.Create();

        await dbContext.InitUserAsync(context.User, CancellationToken.None);
        if (context.Guild != null)
        {
            await dbContext.InitGuildAsync(context.Guild, CancellationToken.None);
            await dbContext.InitGuildUserAsync(context.Guild, context.User as IGuildUser, CancellationToken.None);

            var channelType = DiscordHelper.GetChannelType(context.Channel).Value;
            await dbContext.InitGuildChannelAsync(context.Guild, context.Channel, channelType, CancellationToken.None);
        }

        await dbContext.AddAsync(logItem);
        await dbContext.SaveChangesAsync();
    }

    public async Task LogExecutedInteractionCommandAsync(ICommandInfo command, IInteractionContext context, global::Discord.Interactions.IResult result)
    {
        var data = InteractionCommandExecuted.Create(context.Interaction, command, result);
        var logItem = AuditLogItem.Create(AuditLogItemType.InteractionCommand, context.Guild, context.Channel,
            context.Guild != null ? context.User : null, JsonConvert.SerializeObject(data, JsonSerializerSettings));

        using var dbContext = DbFactory.Create();

        await dbContext.InitUserAsync(context.User, CancellationToken.None);
        if (context.Guild != null)
        {
            await dbContext.InitGuildAsync(context.Guild, CancellationToken.None);

            var channelType = DiscordHelper.GetChannelType(context.Channel).Value;
            await dbContext.InitGuildChannelAsync(context.Guild, context.Channel, channelType, CancellationToken.None);
            await dbContext.InitGuildUserAsync(context.Guild, context.User as IGuildUser, CancellationToken.None);
        }

        await dbContext.AddAsync(logItem);
        await dbContext.SaveChangesAsync();
    }

    private async Task OnUserUnbannedAsync(SocketUser user, SocketGuild guild)
    {
        var auditLog = (await guild.GetAuditLogsAsync(10, actionType: ActionType.Unban).FlattenAsync())
            .FirstOrDefault(o => ((UnbanAuditLogData)o.Data).Target.Id == user.Id);

        if (auditLog == null) return;

        var data = new AuditUserInfo(user);
        var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
        var entity = AuditLogItem.Create(AuditLogItemType.Unban, guild, null, auditLog.User, json, auditLog.Id);

        using var context = DbFactory.Create();

        await context.InitGuildAsync(guild, CancellationToken.None);
        await context.InitUserAsync(auditLog.User, CancellationToken.None);
        await context.InitGuildUserAsync(guild, auditLog.User as IGuildUser ?? guild.GetUser(auditLog.User.Id), CancellationToken.None);

        await context.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    private static bool IsMemberReallyUpdated(SocketGuildUser before, SocketGuildUser after)
    {
        if (before.IsDeafened != after.IsDeafened) return true;
        if (before.IsMuted != after.IsMuted) return true;
        if (before.Nickname != after.Nickname) return true;

        return false;
    }

    private async Task OnMemberUpdatedAsync(SocketGuildUser before, SocketGuildUser after)
    {
        var auditLog = (await after.Guild.GetAuditLogsAsync(10, actionType: ActionType.MemberUpdated).FlattenAsync())
            .FirstOrDefault(o => ((MemberUpdateAuditLogData)o.Data).Target.Id == after.Id);

        if (auditLog == null) return;

        var data = new MemberUpdatedData(before, after);
        var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
        var entity = AuditLogItem.Create(AuditLogItemType.MemberUpdated, after.Guild, null, auditLog.User, json, auditLog.Id);

        using var context = DbFactory.Create();

        await context.InitGuildAsync(after.Guild, CancellationToken.None);
        await context.InitUserAsync(auditLog.User, CancellationToken.None);
        await context.InitGuildUserAsync(after.Guild, auditLog.User as IGuildUser ?? after.Guild.GetUser(auditLog.User.Id), CancellationToken.None);

        await context.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    private async Task OnMemberRolesUpdatedAsync(SocketGuildUser user)
    {
        using var context = DbFactory.Create();
        await context.InitGuildAsync(user.Guild, CancellationToken.None);
        await context.InitUserAsync(user, CancellationToken.None);
        await context.InitGuildUserAsync(user.Guild, user, CancellationToken.None);

        var timeLimit = DateTime.Now.AddDays(-7);
        var auditLogIdsQuery = context.AuditLogs.AsQueryable()
            .Where(o => o.GuildId == user.Guild.Id.ToString() && o.DiscordAuditLogItemId != null && o.Type == AuditLogItemType.MemberRoleUpdated && o.CreatedAt >= timeLimit)
            .Select(o => o.DiscordAuditLogItemId);
        var auditLogIds = (await auditLogIdsQuery.ToListAsync()).SelectMany(o => o.Split(',')).Select(o => Convert.ToUInt64(o)).ToList();

        var logs = (await user.Guild.GetAuditLogsAsync(100, actionType: ActionType.MemberRoleUpdated).FlattenAsync())
            .Where(o => !auditLogIds.Contains(o.Id) && ((MemberRoleAuditLogData)o.Data).Target.Id == user.Id);

        var logData = new Dictionary<ulong, Tuple<List<ulong>, MemberUpdatedData>>();
        foreach (var item in logs)
        {
            if (!logData.ContainsKey(item.User.Id))
                logData.Add(item.User.Id, new Tuple<List<ulong>, MemberUpdatedData>(new List<ulong>(), new MemberUpdatedData(new AuditUserInfo(user))));

            var logItem = logData[item.User.Id];
            logItem.Item1.Add(item.Id);
            logItem.Item2.Merge(item.Data as MemberRoleAuditLogData, user.Guild);
        }

        foreach (var logItem in logData)
        {
            var json = JsonConvert.SerializeObject(logItem.Value.Item2, JsonSerializerSettings);
            var processedUser = user.Guild.GetUser(logItem.Key);
            var discordAuditLogId = string.Join(",", logItem.Value.Item1);
            var entity = AuditLogItem.Create(AuditLogItemType.MemberRoleUpdated, user.Guild, null, processedUser, json, discordAuditLogId);

            await context.InitUserAsync(processedUser, CancellationToken.None);
            await context.InitGuildUserAsync(user.Guild, processedUser, CancellationToken.None);
            await context.AddAsync(entity);
        }

        await context.SaveChangesAsync();
    }

    public virtual async Task<bool> RemoveItemAsync(long id)
    {
        using var context = DbFactory.Create();

        var item = await context.AuditLogs
            .Include(o => o.Files)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (item == null) return false;
        if (item.Files.Count > 0)
        {
            var storage = FileStorageFactory.Create("Audit");

            foreach (var file in item.Files)
            {
                var fileInfo = await storage.GetFileInfoAsync("DeletedAttachments", file.Filename);
                if (!fileInfo.Exists) continue;

                fileInfo.Delete();
            }

            context.RemoveRange(item.Files);
        }

        context.Remove(item);
        return (await context.SaveChangesAsync()) > 0;
    }
}
