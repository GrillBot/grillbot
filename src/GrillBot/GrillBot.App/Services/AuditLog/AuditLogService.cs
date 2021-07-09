using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.FileStorage;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace GrillBot.App.Services.AuditLog
{
    public class AuditLogService : ServiceBase
    {
        private JsonSerializerSettings JsonSerializerSettings { get; }
        private MessageCache.MessageCache MessageCache { get; }
        private FileStorageFactory FileStorageFactory { get; }

        private DateTime NextAllowedChannelUpdateEvent { get; set; }
        private DateTime NextAllowedRoleUpdateEvent { get; set; }

        public AuditLogService(DiscordSocketClient client, GrillBotContextFactory dbFactory, MessageCache.MessageCache cache,
            FileStorageFactory storageFactory) : base(client, dbFactory)
        {
            JsonSerializerSettings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore
            };

            MessageCache = cache;

            DiscordClient.UserLeft += user => user == null || user.Id == DiscordClient.CurrentUser.Id ? Task.CompletedTask : OnUserLeftAsync(user);
            DiscordClient.UserJoined += user => user?.IsUser() != true ? Task.CompletedTask : OnUserJoinedAsync(user);
            DiscordClient.MessageUpdated += (before, after, channel) =>
            {
                if (DiscordClient.Status != UserStatus.Online) return Task.CompletedTask;
                if (channel is not SocketTextChannel textChannel) return Task.CompletedTask;
                return OnMessageEditedAsync(before, after, textChannel);
            };

            DiscordClient.MessageDeleted += (message, channel) =>
            {
                if (DiscordClient.Status != UserStatus.Online) return Task.CompletedTask;
                if (channel is not SocketTextChannel textChannel) return Task.CompletedTask;
                return OnMessageDeletedAsync(message, textChannel);
            };

            DiscordClient.ChannelCreated += channel => channel is SocketGuildChannel guildChannel ? OnChannelCreatedAsync(guildChannel) : Task.CompletedTask;
            DiscordClient.ChannelDestroyed += channel => channel is SocketGuildChannel guildChannel ? OnChannelDeletedAsync(guildChannel) : Task.CompletedTask;
            DiscordClient.ChannelUpdated += async (_before, _after) =>
            {
                if (_before is not SocketGuildChannel before || _after is not SocketGuildChannel after) return;
                if (NextAllowedChannelUpdateEvent > DateTime.Now) return;

                await OnChannelUpdatedAsync(before, after);
                await OnOverwriteChangedAsync(after.Guild, after);
                NextAllowedChannelUpdateEvent = DateTime.Now.AddHours(1);
            };
            DiscordClient.GuildUpdated += (before, after) =>
            {
                if (!before.Emotes.SequenceEqual(after.Emotes))
                    return OnEmotesUpdatedAsync(before, before.Emotes, after.Emotes);

                if (IsGuildReallyUpdated(before, after))
                    return OnGuildUpdatedAsync(before, after);

                return Task.CompletedTask;
            };

            DiscordClient.UserUnbanned += OnUserUnbannedAsync;
            DiscordClient.GuildMemberUpdated += (before, after) =>
            {
                if (IsMemberReallyUpdated(before, after))
                    return OnMemberUpdatedAsync(before, after);

                if (!before.Roles.SequenceEqual(after.Roles) && NextAllowedRoleUpdateEvent <= DateTime.Now)
                {
                    var task = OnMemberRolesUpdatedAsync(after);
                    NextAllowedRoleUpdateEvent = DateTime.Now.AddHours(1);
                    return task;
                }

                return Task.CompletedTask;
            };

            // TODO: Impelement audit log download after restart.

            FileStorageFactory = storageFactory;
        }

        public async Task LogExecutedCommandAsync(CommandInfo command, ICommandContext context, IResult result)
        {
            var data = new CommandExecution(command, context.Message, result);
            var logItem = AuditLogItem.Create(AuditLogItemType.Command, context.Guild, context.Channel, context.User, JsonConvert.SerializeObject(data, JsonSerializerSettings));

            using var dbContext = DbFactory.Create();
            await dbContext.InitGuildAsync(context.Guild);
            await dbContext.InitGuildUserAsync(context.Guild, context.User as IGuildUser);
            await dbContext.InitGuildChannelAsync(context.Guild, context.Channel);

            await dbContext.AddAsync(logItem);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<AuditLogStatItem>> GetStatisticsAsync<TKey>(Expression<Func<AuditLogItem, TKey>> keySelector,
            Func<List<Tuple<TKey, int, DateTime, DateTime>>, IEnumerable<AuditLogStatItem>> converter)
        {
            using var dbContext = DbFactory.Create();

            var query = dbContext.AuditLogs.AsQueryable()
                .GroupBy(keySelector)
                .OrderBy(o => o.Key)
                .Select(o => new Tuple<TKey, int, DateTime, DateTime>(o.Key, o.Count(), o.Min(x => x.CreatedAt), o.Max(x => x.CreatedAt)));

            var data = await query.ToListAsync();
            return converter(data).ToList();
        }

        private async Task OnUserLeftAsync(SocketGuildUser user)
        {
            // Disable logging if bot not have permissions.
            if (!user.Guild.CurrentUser.GuildPermissions.ViewAuditLog) return;

            var ban = await user.Guild.GetBanAsync(user);
            var from = DateTime.UtcNow.AddMinutes(-1);
            RestAuditLogEntry auditLog;
            if (ban != null)
            {
                auditLog = (await user.Guild.GetAuditLogsAsync(5, actionType: ActionType.Ban).FlattenAsync())
                    .FirstOrDefault(o => (o.Data as BanAuditLogData)?.Target.Id == user.Id && o.CreatedAt.DateTime >= from);
            }
            else
            {
                auditLog = (await user.Guild.GetAuditLogsAsync(5, actionType: ActionType.Kick).FlattenAsync())
                    .FirstOrDefault(o => (o.Data as KickAuditLogData)?.Target.Id == user.Id && o.CreatedAt.DateTime >= from);
            }

            var data = new UserLeftGuildData(user.Guild, user, ban != null, ban?.Reason);
            var entity = AuditLogItem.Create(AuditLogItemType.UserLeft, user.Guild, null, auditLog?.User ?? user, JsonConvert.SerializeObject(data, JsonSerializerSettings));

            using var context = DbFactory.Create();

            await context.InitGuildAsync(user.Guild);
            await context.InitUserAsync(user);
            await context.InitGuildUserAsync(user.Guild, user);

            if (auditLog?.User != null)
            {
                await context.InitUserAsync(auditLog.User);
                await context.InitGuildUserAsync(user.Guild, user.Guild.GetUser(auditLog.User.Id));
            }

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        private async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            var data = new UserJoinedAuditData(user.Guild);
            var entity = AuditLogItem.Create(AuditLogItemType.UserJoined, user.Guild, null, user, JsonConvert.SerializeObject(data, JsonSerializerSettings));

            using var context = DbFactory.Create();

            await context.InitGuildAsync(user.Guild);
            await context.InitUserAsync(user);
            await context.InitGuildUserAsync(user.Guild, user);

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        private async Task OnMessageEditedAsync(Cacheable<IMessage, ulong> before, SocketMessage after, SocketTextChannel channel)
        {
            var oldMessage = before.HasValue ? before.Value : MessageCache.GetMessage(before.Id);
            if (oldMessage == null || after == null || !oldMessage.Author.IsUser() || oldMessage.Content == after.Content) return;

            var data = new MessageEditedData(oldMessage, after);
            var entity = AuditLogItem.Create(AuditLogItemType.MessageEdited, channel.Guild, channel, after.Author, JsonConvert.SerializeObject(data, JsonSerializerSettings));

            using var context = DbFactory.Create();

            await context.InitGuildAsync(channel.Guild);
            await context.InitUserAsync(after.Author);
            await context.InitGuildUserAsync(channel.Guild, after.Author as IGuildUser ?? channel.Guild.GetUser(after.Author.Id));

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
            MessageCache.MarkUpdated(after.Id);
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, SocketTextChannel channel)
        {
            if ((message.HasValue ? message.Value : MessageCache.GetMessage(message.Id, true)) is not IUserMessage deletedMessage) return;

            var timeLimit = DateTime.UtcNow.AddMinutes(-1);
            var auditLog = (await channel.Guild.GetAuditLogsAsync(5, actionType: ActionType.MessageDeleted).FlattenAsync())
                .Where(o => o.CreatedAt.DateTime >= timeLimit)
                .FirstOrDefault(o =>
                {
                    var data = (MessageDeleteAuditLogData)o.Data;
                    return data.Target.Id == deletedMessage.Author.Id && data.ChannelId == channel.Id;
                });

            var data = new MessageDeletedData(deletedMessage);
            var removedBy = auditLog?.User ?? deletedMessage.Author;
            var entity = AuditLogItem.Create(AuditLogItemType.MessageDeleted, channel.Guild, channel, removedBy, JsonConvert.SerializeObject(data, JsonSerializerSettings));

            if (deletedMessage.Attachments.Count > 0)
            {
                var storage = FileStorageFactory.Create("Audit");

                foreach (var attachment in deletedMessage.Attachments.Where(o => o.Size <= 10 * 1024 * 1024)) // Max 10MB per file
                {
                    var content = await attachment.DownloadAsync();
                    if (content == null) continue;

                    var fileEntity = new AuditLogFileMeta
                    {
                        Filename = attachment.Filename,
                        Size = attachment.Size
                    };

                    var filename = string.Join("_", new[]
                    {
                        fileEntity.FilenameWithoutExtension,
                        attachment.Id.ToString(),
                        deletedMessage.Author.Id.ToString()
                    }) + fileEntity.Extension;

                    await storage.StoreFileAsync("DeletedAttachments", filename, content);
                    entity.Files.Add(fileEntity);
                }
            }

            using var context = DbFactory.Create();

            await context.InitGuildAsync(channel.Guild);
            await context.InitGuildChannelAsync(channel.Guild, channel);
            await context.InitUserAsync(removedBy);
            await context.InitGuildUserAsync(channel.Guild, removedBy as IGuildUser ?? channel.Guild.GetUser(removedBy.Id));

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        private async Task OnChannelCreatedAsync(SocketGuildChannel channel)
        {
            var auditLog = (await channel.Guild.GetAuditLogsAsync(50, actionType: ActionType.ChannelCreated).FlattenAsync())
                .FirstOrDefault(o => ((ChannelCreateAuditLogData)o.Data).ChannelId == channel.Id);

            if (auditLog == null) return;

            var data = new AuditChannelInfo(auditLog.Data as ChannelCreateAuditLogData);
            var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
            var entity = AuditLogItem.Create(AuditLogItemType.ChannelCreated, channel.Guild, channel, auditLog.User, json, auditLog.Id);

            using var context = DbFactory.Create();

            await context.InitGuildAsync(channel.Guild);
            await context.InitGuildChannelAsync(channel.Guild, channel);
            await context.InitUserAsync(auditLog.User);
            await context.InitGuildUserAsync(channel.Guild, auditLog.User as IGuildUser ?? channel.Guild.GetUser(auditLog.User.Id));

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        private async Task OnChannelDeletedAsync(SocketGuildChannel channel)
        {
            var auditLog = (await channel.Guild.GetAuditLogsAsync(50, actionType: ActionType.ChannelDeleted).FlattenAsync())
                .FirstOrDefault(o => ((ChannelDeleteAuditLogData)o.Data).ChannelId == channel.Id);

            if (auditLog == null) return;

            var data = new AuditChannelInfo(auditLog.Data as ChannelDeleteAuditLogData, (channel as SocketTextChannel)?.Topic);
            var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
            var entity = AuditLogItem.Create(AuditLogItemType.ChannelDeleted, channel.Guild, channel, auditLog.User, json, auditLog.Id);

            using var context = DbFactory.Create();

            await context.InitGuildAsync(channel.Guild);
            await context.InitGuildChannelAsync(channel.Guild, channel);
            await context.InitUserAsync(auditLog.User);
            await context.InitGuildUserAsync(channel.Guild, auditLog.User as IGuildUser ?? channel.Guild.GetUser(auditLog.User.Id));

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        private async Task OnChannelUpdatedAsync(SocketGuildChannel before, SocketGuildChannel after)
        {
            if (before.IsEqual(after)) return;

            var auditLog = (await after.Guild.GetAuditLogsAsync(50, actionType: ActionType.ChannelUpdated).FlattenAsync())
                .FirstOrDefault(o => ((ChannelUpdateAuditLogData)o.Data).ChannelId == after.Id);

            if (auditLog == null) return;

            var auditData = auditLog.Data as ChannelUpdateAuditLogData;
            var data = new Diff<AuditChannelInfo>(new(before.Id, auditData.Before), new(after.Id, auditData.After));
            var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
            var entity = AuditLogItem.Create(AuditLogItemType.ChannelUpdated, after.Guild, after, auditLog.User, json, auditLog.Id);

            using var context = DbFactory.Create();

            await context.InitGuildAsync(after.Guild);
            await context.InitGuildChannelAsync(after.Guild, after);
            await context.InitUserAsync(auditLog.User);
            await context.InitGuildUserAsync(after.Guild, auditLog.User as IGuildUser ?? after.Guild.GetUser(auditLog.User.Id));

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        private async Task OnEmotesUpdatedAsync(SocketGuild guild, IReadOnlyCollection<GuildEmote> before, IReadOnlyCollection<GuildEmote> after)
        {
            (List<GuildEmote> added, List<GuildEmote> removed) = new Func<(List<GuildEmote>, List<GuildEmote>)>(() =>
            {
                var removed = before.Where(e => !after.Contains(e)).ToList();
                var added = after.Where(e => !before.Contains(e)).ToList();
                return (added, removed);
            })();

            if (removed.Count == 0) return;

            var auditLog = (await guild.GetAuditLogsAsync(DiscordConfig.MaxAuditLogEntriesPerBatch, actionType: ActionType.EmojiDeleted).FlattenAsync())
                .FirstOrDefault(o => removed.Any(x => x.Id == ((EmoteDeleteAuditLogData)o.Data).EmoteId));

            var data = new AuditEmoteInfo(auditLog.Data as EmoteDeleteAuditLogData);
            var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
            var entity = AuditLogItem.Create(AuditLogItemType.EmojiDeleted, guild, null, auditLog.User, json, auditLog.Id);

            using var context = DbFactory.Create();

            await context.InitGuildAsync(guild);
            await context.InitUserAsync(auditLog.User);
            await context.InitGuildUserAsync(guild, auditLog.User as IGuildUser ?? guild.GetUser(auditLog.User.Id));

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        private async Task OnOverwriteChangedAsync(SocketGuild guild, SocketGuildChannel channel)
        {
            using var context = DbFactory.Create();
            await context.InitGuildAsync(guild);
            await context.InitGuildChannelAsync(guild, channel);

            var timeLimit = DateTime.Now.AddDays(-5);
            var auditLogIdsQuery = context.AuditLogs.AsQueryable()
                .Where(o => o.GuildId == guild.Id.ToString() && o.DiscordAuditLogItemId != null && o.ChannelId == channel.Id.ToString() && o.CreatedAt >= timeLimit)
                .Select(o => o.DiscordAuditLogItemId);
            var auditLogIds = (await auditLogIdsQuery.ToListAsync()).ConvertAll(o => Convert.ToUInt64(o));

            var logs = (await guild.GetAuditLogsAsync(100).FlattenAsync())
                .Where(o => (o.Action == ActionType.OverwriteCreated || o.Action == ActionType.OverwriteDeleted || o.Action == ActionType.OverwriteUpdated) && !auditLogIds.Contains(o.Id))
                .ToList();

            var created = logs.FindAll(o => o.Action == ActionType.OverwriteCreated && ((OverwriteCreateAuditLogData)o.Data).ChannelId == channel.Id);
            var removed = logs.FindAll(o => o.Action == ActionType.OverwriteDeleted && ((OverwriteDeleteAuditLogData)o.Data).ChannelId == channel.Id);
            var updated = logs.FindAll(o => o.Action == ActionType.OverwriteUpdated && ((OverwriteUpdateAuditLogData)o.Data).ChannelId == channel.Id);

            foreach (var log in created)
            {
                var data = new AuditOverwriteInfo(((OverwriteCreateAuditLogData)log.Data).Overwrite);
                var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
                var entity = AuditLogItem.Create(AuditLogItemType.OverwriteCreated, guild, channel, log.User, json, log.Id);

                await context.InitUserAsync(log.User);
                await context.InitGuildUserAsync(guild, log.User as IGuildUser ?? guild.GetUser(log.User.Id));
                await context.AddAsync(entity);
            }

            foreach (var log in removed)
            {
                var data = new AuditOverwriteInfo(((OverwriteDeleteAuditLogData)log.Data).Overwrite);
                var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
                var entity = AuditLogItem.Create(AuditLogItemType.OverwriteDeleted, guild, channel, log.User, json, log.Id);

                await context.InitUserAsync(log.User);
                await context.InitGuildUserAsync(guild, log.User as IGuildUser ?? guild.GetUser(log.User.Id));
                await context.AddAsync(entity);
            }

            foreach (var log in updated)
            {
                var auditData = (OverwriteUpdateAuditLogData)log.Data;
                var oldPerms = new Overwrite(auditData.OverwriteTargetId, auditData.OverwriteType, auditData.OldPermissions);
                var newPerms = new Overwrite(auditData.OverwriteTargetId, auditData.OverwriteType, auditData.NewPermissions);
                var data = new Diff<AuditOverwriteInfo>(new(oldPerms), new(newPerms));
                var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
                var entity = AuditLogItem.Create(AuditLogItemType.OverwriteUpdated, guild, channel, log.User, json, log.Id);

                await context.InitUserAsync(log.User);
                await context.InitGuildUserAsync(guild, log.User as IGuildUser ?? guild.GetUser(log.User.Id));
                await context.AddAsync(entity);
            }

            await context.SaveChangesAsync();
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

            await context.InitGuildAsync(guild);
            await context.InitUserAsync(auditLog.User);
            await context.InitGuildUserAsync(guild, auditLog.User as IGuildUser ?? guild.GetUser(auditLog.User.Id));

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

            await context.InitGuildAsync(after.Guild);
            await context.InitUserAsync(auditLog.User);
            await context.InitGuildUserAsync(after.Guild, auditLog.User as IGuildUser ?? after.Guild.GetUser(auditLog.User.Id));

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        private async Task OnMemberRolesUpdatedAsync(SocketGuildUser user)
        {
            using var context = DbFactory.Create();
            await context.InitGuildAsync(user.Guild);
            await context.InitUserAsync(user);
            await context.InitGuildUserAsync(user.Guild, user);

            var timeLimit = DateTime.Now.AddDays(-5);
            var auditLogIdsQuery = context.AuditLogs.AsQueryable()
                .Where(o => o.GuildId == user.Guild.Id.ToString() && o.DiscordAuditLogItemId != null && o.Type == AuditLogItemType.MemberRoleUpdated && o.CreatedAt >= timeLimit)
                .Select(o => o.DiscordAuditLogItemId);
            var auditLogIds = (await auditLogIdsQuery.ToListAsync()).ConvertAll(o => Convert.ToUInt64(o));

            var logs = (await user.Guild.GetAuditLogsAsync(100, actionType: ActionType.MemberRoleUpdated).FlattenAsync())
                .Where(o => !auditLogIds.Contains(o.Id) && ((MemberRoleAuditLogData)o.Data).Target.Id == user.Id);

            foreach (var item in logs)
            {
                var data = new MemberUpdatedData(item.Data as MemberRoleAuditLogData, user.Guild);
                var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
                var entity = AuditLogItem.Create(AuditLogItemType.MemberRoleUpdated, user.Guild, null, item.User, json, item.Id);

                await context.InitUserAsync(item.User);
                await context.InitGuildUserAsync(user.Guild, item.User as IGuildUser ?? user.Guild.GetUser(item.User.Id));
                await context.AddAsync(entity);
            }

            await context.SaveChangesAsync();
        }

        private static bool IsGuildReallyUpdated(SocketGuild before, SocketGuild after)
        {
            if (before.DefaultMessageNotifications != after.DefaultMessageNotifications) return true;
            if (before.Description != after.Description) return true;
            if (before.VanityURLCode != after.VanityURLCode) return true;
            if (before.BannerId != after.BannerId) return true;
            if (before.DiscoverySplashId != after.DiscoverySplashId) return true;
            if (before.SplashId != after.SplashId) return true;
            if (before.IconId != after.IconId) return true;
            if (before.VoiceRegionId != after.VoiceRegionId) return true;
            if (before.OwnerId != after.OwnerId) return true;
            if (before.PublicUpdatesChannel?.Id != after.PublicUpdatesChannel?.Id) return true;
            if (before.RulesChannel?.Id != after.RulesChannel?.Id) return true;
            if (before.SystemChannel?.Id != after.SystemChannel?.Id) return true;
            if (before.AFKChannel?.Id != after.AFKChannel?.Id) return true;
            if (before.AFKTimeout != after.AFKTimeout) return true;
            if (before.Name != after.Name) return true;
            if (before.MfaLevel != after.MfaLevel) return true;

            return false;
        }

        private async Task OnGuildUpdatedAsync(SocketGuild before, SocketGuild after)
        {
            var auditLog = (await after.GetAuditLogsAsync(100, actionType: ActionType.GuildUpdated).FlattenAsync())
                .OrderByDescending(o => o.CreatedAt.DateTime)
                .FirstOrDefault();

            var data = new GuildUpdatedData(before, after);
            var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
            var entity = AuditLogItem.Create(AuditLogItemType.GuildUpdated, after, null, auditLog.User, json, auditLog.Id);

            using var context = DbFactory.Create();

            await context.InitGuildAsync(after);
            await context.InitUserAsync(auditLog.User);
            await context.InitGuildUserAsync(after, auditLog.User as IGuildUser ?? after.GetUser(auditLog.User.Id));

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }
    }
}
