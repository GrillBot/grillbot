using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.Invite;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Services
{
    public class InviteService : ServiceBase
    {
        public ConcurrentBag<InviteMetadata> MetadataCache { get; }

        public InviteService(DiscordSocketClient discordClient, GrillBotContextFactory dbFactory) : base(discordClient, dbFactory)
        {
            MetadataCache = new ConcurrentBag<InviteMetadata>();

            DiscordClient.Ready += () => InitAsync();
            DiscordClient.UserJoined += (user) => user.IsUser() ? OnUserJoinedAsync(user) : Task.CompletedTask;
        }

        public async Task InitAsync()
        {
            using var dbContext = DbFactory.Create();

            var botId = DiscordClient.CurrentUser.Id.ToString();
            await dbContext.InitUserAsync(DiscordClient.CurrentUser);

            var invites = new List<InviteMetadata>();
            foreach (var guild in DiscordClient.Guilds)
            {
                var guildInvites = await GetLatestMetadataOfGuildAsync(guild);
                if (guildInvites == null) continue;

                await dbContext.InitGuildAsync(guild);
                await dbContext.InitGuildUserAsync(guild, guild.CurrentUser);

                var logItem = new AuditLogItem()
                {
                    CreatedAt = DateTime.Now,
                    Data = $"Invites for guild {guild.Name} ({guild.Id}) loaded. Loaded invites: {guildInvites.Count}",
                    GuildId = guild.Id.ToString(),
                    Type = AuditLogItemType.Info,
                    ProcessedUserId = botId
                };

                await dbContext.AddAsync(logItem);
                invites.AddRange(guildInvites);
            }

            MetadataCache.Clear();
            invites.ForEach(invite => MetadataCache.Add(invite));
            await dbContext.SaveChangesAsync();
        }

        public async Task<int> RefreshMetadataOfGuildAsync(SocketGuild guild)
        {
            var latestMetadata = await GetLatestMetadataOfGuildAsync(guild);
            UpdateInvitesCache(latestMetadata, guild);

            return latestMetadata.Count;
        }

        static private async Task<List<InviteMetadata>> GetLatestMetadataOfGuildAsync(SocketGuild guild)
        {
            if (!guild.CurrentUser.GuildPermissions.CreateInstantInvite && guild.CurrentUser.GuildPermissions.ManageGuild)
                return null;

            var invites = new List<InviteMetadata>();

            if (!string.IsNullOrEmpty(guild.VanityURLCode))
            {
                // Vanity invite not returns uses, but in library github is merged fix (PR #1832). TODO: Update library
                var vanityInvite = await guild.GetVanityInviteAsync();
                if (vanityInvite != null)
                    invites.Add(InviteMetadata.FromDiscord(vanityInvite));
            }

            var guildInvites = (await guild.GetInvitesAsync()).Select(InviteMetadata.FromDiscord);
            invites.AddRange(guildInvites);

            return invites;
        }

        public async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            var latestInvites = await GetLatestMetadataOfGuildAsync(user.Guild);
            var usedInvite = FindUsedInvite(user.Guild, latestInvites);
            await SetInviteToUserAsync(user, user.Guild, usedInvite, latestInvites);
        }

        private async Task SetInviteToUserAsync(SocketGuildUser user, SocketGuild guild, InviteMetadata usedInvite, List<InviteMetadata> latestInvites)
        {
            var guildId = guild.Id.ToString();
            var userId = user.Id.ToString();

            using var dbContext = DbFactory.Create();

            await dbContext.InitGuildAsync(guild);
            await dbContext.InitUserAsync(user);

            var joinedUserEntity = await dbContext.GuildUsers.AsQueryable()
                .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == userId);

            if (joinedUserEntity == null)
            {
                joinedUserEntity = GuildUser.FromDiscord(guild, user);
                await dbContext.AddAsync(joinedUserEntity);
            }

            if (usedInvite == null)
            {
                var logItem = new AuditLogItem()
                {
                    CreatedAt = DateTime.Now,
                    Data = $"User {user.GetFullName()} ({user.Id}) used unknown invite.",
                    GuildId = guildId,
                    ProcessedUserId = userId,
                    Type = AuditLogItemType.Warning
                };

                await dbContext.AddAsync(logItem);
            }
            else
            {
                if (usedInvite.CreatorId != null)
                {
                    var creatorUser = guild.GetUser(usedInvite.CreatorId.Value);

                    await dbContext.InitUserAsync(creatorUser);
                    await dbContext.InitGuildUserAsync(guild, creatorUser);
                }

                var invite = await dbContext.Invites.AsQueryable()
                    .FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.Code == usedInvite.Code);

                if (invite == null)
                {
                    invite = usedInvite.ToEntity();
                    await dbContext.AddAsync(invite);
                }

                joinedUserEntity.UsedInviteCode = usedInvite.Code;
            }

            await dbContext.SaveChangesAsync();
            UpdateInvitesCache(latestInvites, guild);
        }

        private void UpdateInvitesCache(List<InviteMetadata> invites, SocketGuild guild)
        {
            invites.AddRange(MetadataCache.Where(o => o.GuildId != guild.Id));

            MetadataCache.Clear();
            invites.ForEach(invite => MetadataCache.Add(invite));
        }

        private InviteMetadata FindUsedInvite(SocketGuild guild, List<InviteMetadata> latestData)
        {
            var result = MetadataCache
                .Where(o => o.GuildId == guild.Id)
                .FirstOrDefault(inv =>
                {
                    var fromLatest = latestData.Find(x => x.Code == inv.Code);
                    return fromLatest != null && fromLatest.Uses > inv.Uses;
                });

            return result ?? latestData.Find(inv => !MetadataCache.Any(o => o.Code == inv.Code));
        }

        public async Task AssignInviteToUserAsync(SocketGuildUser user, SocketGuild guild, string code)
        {
            var latestInvites = await GetLatestMetadataOfGuildAsync(guild);
            var usedInvite = latestInvites.Find(o => o.Code == code);

            if (usedInvite == null)
                throw new NotFoundException($"Pozvánka s kódem `{code}` neexistuje.");

            await SetInviteToUserAsync(user, guild, usedInvite, latestInvites);
        }
    }
}
