using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.Invite;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Services
{
    public class InviteService
    {
        private ConcurrentBag<InviteMetadata> MetadataCache { get; }
        private DiscordSocketClient DiscordClient { get; }
        private ILogger<InviteService> Logger { get; }
        private GrillBotContextFactory DbFactory { get; }

        public InviteService(DiscordSocketClient discordClient, ILogger<InviteService> logger, GrillBotContextFactory dbFactory)
        {
            MetadataCache = new ConcurrentBag<InviteMetadata>();

            DiscordClient = discordClient;
            Logger = logger;
            DbFactory = dbFactory;

            DiscordClient.Ready += () => InitAsync();
            DiscordClient.UserJoined += (user) => user.IsUser() ? OnUserJoinedAsync(user) : Task.CompletedTask;
        }

        public async Task InitAsync()
        {
            using var dbContext = DbFactory.Create();

            var botId = DiscordClient.CurrentUser.Id.ToString();
            if (!await dbContext.Users.AsQueryable().AnyAsync(o => o.Id == botId))
                await dbContext.AddAsync(new User() { Id = botId });

            var invites = new List<InviteMetadata>();
            foreach (var guild in DiscordClient.Guilds.Where(o => o.CurrentUser.GuildPermissions.CreateInstantInvite && o.CurrentUser.GuildPermissions.ManageGuild))
            {
                if (!await dbContext.Guilds.AsQueryable().AnyAsync(o => o.Id == guild.Id.ToString()))
                    await dbContext.AddAsync(new Guild() { Id = guild.Id.ToString() });

                if (!await dbContext.GuildUsers.AsQueryable().AnyAsync(o => o.GuildId == guild.Id.ToString() && o.UserId == botId))
                    await dbContext.AddAsync(new GuildUser() { GuildId = guild.Id.ToString(), UserId = botId });

                var guildInvites = await GetLatestMetadataOfGuildAsync(guild);

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
            // TODO: Ignore loading if bot not have permissions.
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

        private async Task SetInviteToUserAsync(SocketUser user, SocketGuild guild, InviteMetadata usedInvite, List<InviteMetadata> latestInvites)
        {
            if (usedInvite == null)
            {
                // TODO: Log as warning to audit log.
                Logger.LogWarning($"User {user.GetFullName()} ({user.Id}) used unknown invite.");
                return;
            }

            using var dbContext = DbFactory.Create();

            if (!await dbContext.Guilds.AsQueryable().AnyAsync(o => o.Id == guild.Id.ToString()))
                await dbContext.AddAsync(new Guild() { Id = guild.Id.ToString() });

            if (usedInvite.CreatorId != null)
            {
                var creatorBaseEntityExists = await dbContext.Users.AsQueryable().AnyAsync(o => o.Id == usedInvite.CreatorId.Value.ToString());

                var creatorGuildEntityExists = await dbContext.GuildUsers.AsQueryable()
                    .AnyAsync(o => o.UserId == usedInvite.CreatorId.Value.ToString() && o.GuildId == guild.Id.ToString());

                if (!creatorBaseEntityExists)
                    await dbContext.AddAsync(new User() { Id = usedInvite.CreatorId.Value.ToString() });

                if (!creatorGuildEntityExists)
                    await dbContext.AddAsync(new GuildUser() { GuildId = guild.Id.ToString(), UserId = usedInvite.CreatorId.Value.ToString() });
            }

            var invite = await dbContext.Invites.AsQueryable()
                .FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.Code == usedInvite.Code);

            if (invite == null)
            {
                invite = usedInvite.ToEntity();
                await dbContext.AddAsync(invite);
            }

            if (!await dbContext.Users.AsQueryable().AnyAsync(o => o.Id == user.Id.ToString()))
                await dbContext.AddAsync(new User() { Id = user.Id.ToString() });

            var joinedUserEntity = await dbContext.GuildUsers
                .AsQueryable()
                .FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.UserId == user.Id.ToString());

            if (joinedUserEntity == null)
            {
                joinedUserEntity = new GuildUser()
                {
                    UserId = user.Id.ToString(),
                    GuildId = guild.Id.ToString(),
                };

                await dbContext.AddAsync(joinedUserEntity);
            }

            joinedUserEntity.UsedInviteCode = usedInvite.Code;
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

        public async Task AssignInviteToUserAsync(SocketUser user, SocketGuild guild, string code)
        {
            var latestInvites = await GetLatestMetadataOfGuildAsync(guild);
            var usedInvite = latestInvites.Find(o => o.Code == code);

            if (usedInvite == null)
                throw new NotFoundException($"Pozvánka s kódem `{code}` neexistuje.");

            await SetInviteToUserAsync(user, guild, usedInvite, latestInvites);
        }
    }
}
