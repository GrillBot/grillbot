using Discord;
using Discord.Net;
using Discord.WebSocket;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Logging;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.App.Services.Unverify
{
    public class UnverifyService : ServiceBase
    {
        private UnverifyChecker Checker { get; }
        private UnverifyProfileGenerator ProfileGenerator { get; }
        private UnverifyLogger Logger { get; }
        private LoggingService Logging { get; }

        public UnverifyService(DiscordSocketClient client, UnverifyChecker checker, UnverifyProfileGenerator profileGenerator,
            UnverifyLogger logger, GrillBotContextFactory dbFactory, LoggingService logging) : base(client, dbFactory)
        {
            Checker = checker;
            ProfileGenerator = profileGenerator;
            Logger = logger;
            Logging = logging;

            DiscordClient.UserLeft += OnUserLeftAsync;
        }

        public async Task<List<string>> SetUnverifyAsync(List<SocketUser> users, DateTime end, string data, SocketGuild guild, IUser from, bool dry)
        {
            await guild.DownloadUsersAsync();
            var messages = new List<string>();

            var muteRole = await GetMutedRoleAsync(guild);
            var fromUser = from as IGuildUser ?? guild.GetUser(from.Id);

            Checker.ValidateUnverifyGroup(users);
            foreach (var user in users)
            {
                var message = await SetUnverifyAsync(user, end, data, guild, fromUser, false, new List<string>(), muteRole, dry);
                messages.Add(message);
            }

            return messages;
        }

        public async Task<string> SetUnverifyAsync(SocketUser user, DateTime end, string data, SocketGuild guild, IGuildUser from, bool selfunverify, List<string> keep,
            IRole muteRole, bool dry)
        {
            if (selfunverify && muteRole == null) muteRole = await GetMutedRoleAsync(guild);
            var guildUser = user as SocketGuildUser ?? guild.GetUser(user.Id);

            await Checker.ValidateUnverifyAsync(guildUser, guild, selfunverify, end, keep?.Count ?? 0);
            var profile = await ProfileGenerator.CreateAsync(guildUser, guild, end, data, selfunverify, keep, muteRole);

            if (dry)
                return UnverifyMessageGenerator.CreateUnverifyMessageToChannel(profile);

            var unverifyLog = await LogUnverifyAsync(profile, guild, from, selfunverify, CancellationToken.None);
            try
            {
                await profile.Destination.TryAddRoleAsync(muteRole);
                await profile.RemoveRolesAsync();
                await profile.ReturnChannelsAsync(guild);

                using var context = DbFactory.Create();

                await context.InitGuildAsync(guild, CancellationToken.None);
                await context.InitUserAsync(from, CancellationToken.None);
                await context.InitGuildUserAsync(guild, from, CancellationToken.None);

                if (from != profile.Destination)
                {
                    await context.InitUserAsync(profile.Destination, CancellationToken.None);
                    await context.InitGuildUserAsync(guild, profile.Destination, CancellationToken.None);
                }

                var dbGuildUser = await context.GuildUsers.Include(o => o.Unverify)
                    .FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.UserId == profile.Destination.Id.ToString());

                dbGuildUser.Unverify = new Database.Entity.Unverify()
                {
                    Channels = profile.ChannelsToRemove.ConvertAll(o => new GuildChannelOverride() { AllowValue = o.AllowValue, ChannelId = o.ChannelId, DenyValue = o.DenyValue }),
                    EndAt = profile.End,
                    GuildId = guild.Id.ToString(),
                    Reason = profile.Reason,
                    Roles = profile.RolesToRemove.ConvertAll(o => o.Id.ToString()),
                    SetOperationId = unverifyLog.Id,
                    StartAt = profile.Start,
                    UserId = profile.Destination.Id.ToString()
                };

                await context.SaveChangesAsync();

                try
                {
                    var dmMessage = UnverifyMessageGenerator.CreateUnverifyPMMessage(profile, guild);
                    await profile.Destination.SendMessageAsync(dmMessage);
                }
                catch (HttpException ex) when (ex.DiscordCode == 50007)
                {
                    // User have disabled DMs.
                }

                return UnverifyMessageGenerator.CreateUnverifyMessageToChannel(profile);
            }
            catch (Exception ex)
            {
                await profile.Destination.TryRemoveRoleAsync(muteRole);
                await profile.ReturnRolesAsync();
                await profile.ReturnChannelsAsync(guild);
                await Logging.ErrorAsync("Unverify", $"An error occured when unverify removing access to {user.GetFullName()}", ex);
                return UnverifyMessageGenerator.CreateUnverifyFailedToChannel(profile.Destination);
            }
        }

        private async Task<IRole> GetMutedRoleAsync(IGuild guild)
        {
            using var context = DbFactory.Create();

            var dbGuild = await context.Guilds.AsNoTracking().FirstOrDefaultAsync(o => o.Id == guild.Id.ToString());
            return string.IsNullOrEmpty(dbGuild?.MuteRoleId) ? null : guild.GetRole(Convert.ToUInt64(dbGuild.MuteRoleId));
        }

        private Task<UnverifyLog> LogUnverifyAsync(UnverifyUserProfile profile, SocketGuild guild, IGuildUser from, bool selfunverify,
            CancellationToken cancellationToken)
        {
            if (selfunverify)
                return Logger.LogSelfunverifyAsync(profile, guild, cancellationToken);

            return Logger.LogUnverifyAsync(profile, guild, from, cancellationToken);
        }

        public async Task<string> UpdateUnverifyAsync(IGuildUser user, SocketGuild guild, DateTime newEnd, IGuildUser fromUser)
        {
            using var context = DbFactory.Create();

            var dbUser = await context.GuildUsers.Include(o => o.Unverify)
                .FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.UserId == user.Id.ToString());

            if (dbUser?.Unverify == null)
                throw new NotFoundException("Aktualizace času nelze pro hledaného uživatele provést. Unverify nenalezeno.");

            if ((dbUser.Unverify.EndAt - DateTime.Now).TotalSeconds <= 30.0)
                throw new ValidationException("Aktualizace data a času již není možná. Vypršel čas, nebo zbývá méně, než půl minuty.");

            await Logger.LogUpdateAsync(DateTime.Now, newEnd, guild, fromUser, user, CancellationToken.None);

            dbUser.Unverify.EndAt = newEnd;
            dbUser.Unverify.StartAt = DateTime.Now;
            await context.SaveChangesAsync();

            try
            {
                var dmMessage = UnverifyMessageGenerator.CreateUpdatePMMessage(guild, newEnd);
                await user.SendMessageAsync(dmMessage);
            }
            catch (HttpException ex) when (ex.DiscordCode == 50007)
            {
                // User have disabled DMs.
            }

            return UnverifyMessageGenerator.CreateUpdateChannelMessage(user, newEnd);
        }

        public async Task<string> RemoveUnverifyAsync(SocketGuild guild, IGuildUser fromUser, IGuildUser toUser, bool isAuto = false, CancellationToken cancellationToken = default)
        {
            try
            {
                using var context = DbFactory.Create();

                var dbUser = await context.GuildUsers
                    .Include(o => o.Unverify).ThenInclude(o => o.UnverifyLog)
                    .FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.UserId == toUser.Id.ToString(), cancellationToken);

                if (dbUser?.Unverify == null)
                    return UnverifyMessageGenerator.CreateRemoveAccessUnverifyNotFound(toUser);

                var profile = ProfileGenerator.Reconstruct(dbUser.Unverify.UnverifyLog, toUser, guild);
                await LogRemoveAsync(profile.RolesToRemove, profile.ChannelsToRemove, toUser, guild, fromUser, isAuto, cancellationToken);

                var muteRole = await GetMutedRoleAsync(guild);
                if (muteRole != null && profile.Destination.RoleIds.Contains(muteRole.Id))
                    await profile.Destination.RemoveRoleAsync(muteRole);

                await profile.ReturnChannelsAsync(guild);
                await profile.ReturnRolesAsync();

                dbUser.Unverify = null;
                await context.SaveChangesAsync(cancellationToken);

                if (!isAuto)
                {
                    try
                    {
                        var dmMessage = UnverifyMessageGenerator.CreateRemoveAccessManuallyPMMessage(guild);
                        await toUser.SendMessageAsync(dmMessage);
                    }
                    catch (HttpException ex) when (ex.DiscordCode == 50007)
                    {
                        // User have disabled DMs.
                    }
                }

                return UnverifyMessageGenerator.CreateRemoveAccessManuallyToChannel(toUser);
            }
            catch (Exception ex) when (!isAuto)
            {
                await Logging.ErrorAsync("Unverify/Remove", "An error occured when unverify returning access.", ex);
                return UnverifyMessageGenerator.CreateRemoveAccessManuallyFailed(toUser, ex);
            }
        }

        private Task LogRemoveAsync(List<IRole> returnedRoles, List<ChannelOverride> channels, IGuildUser user, IGuild guild,
            IGuildUser fromUser, bool isAuto, CancellationToken cancellationToken)
        {
            if (isAuto)
                return Logger.LogAutoremoveAsync(returnedRoles, channels, user, guild, cancellationToken);

            return Logger.LogRemoveAsync(returnedRoles, channels, guild, fromUser, user, cancellationToken);
        }

        public async Task UnverifyAutoremoveAsync(ulong guildId, ulong userId, CancellationToken cancellationToken)
        {
            using var context = DbFactory.Create();

            try
            {
                var unverify = await context.Unverifies.AsQueryable()
                    .FirstOrDefaultAsync(o => o.UserId == userId.ToString() && o.GuildId == guildId.ToString(), cancellationToken);

                if (unverify == null) return;
                var guild = DiscordClient.GetGuild(guildId);

                if (guild == null)
                {
                    context.Remove(unverify);
                    return;
                }

                await guild.DownloadUsersAsync();
                var user = guild.GetUser(userId);

                if (user == null)
                {
                    context.Remove(unverify);
                    return;
                }

                await RemoveUnverifyAsync(guild, guild.CurrentUser, user, true, cancellationToken);
            }
            catch (Exception ex)
            {
                await Logging.ErrorAsync("Unverify/Autoremove", $"An error occured when unverify returning access ({guildId}/{userId}).", ex);
            }
            finally
            {
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task OnUserLeftAsync(SocketGuildUser user)
        {
            using var context = DbFactory.Create();

            var unverify = await context.Unverifies.AsQueryable().FirstOrDefaultAsync(o => o.GuildId == user.Guild.Id.ToString() && o.UserId == user.Id.ToString());

            if (unverify != null)
            {
                context.Remove(unverify);
                await context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnverifyCountsOfGuildAsync(IGuild guild)
        {
            using var context = DbFactory.Create();

            return await context.Unverifies.AsQueryable()
                .CountAsync(o => o.GuildId == guild.Id.ToString());
        }

        public async Task<UnverifyUserProfile> GetCurrentUnverifyAsync(SocketGuild guild, int page)
        {
            await guild.DownloadUsersAsync();
            using var context = DbFactory.Create();

            var unverify = await context.Unverifies
                .Include(o => o.UnverifyLog)
                .Where(o => o.GuildId == guild.Id.ToString())
                .Skip(page)
                .FirstOrDefaultAsync();

            if (unverify == null)
                return null;

            var user = guild.GetUser(Convert.ToUInt64(unverify.UserId));
            return ProfileGenerator.Reconstruct(unverify.UnverifyLog, user, guild);
        }

        public async Task<List<UnverifyUserProfile>> GetAllUnverifiesOfGuildAsync(SocketGuild guild)
        {
            await guild.DownloadUsersAsync();
            using var context = DbFactory.Create();

            var unverifies = await context.Unverifies
                .AsNoTracking()
                .Include(o => o.UnverifyLog)
                .Where(o => o.GuildId == guild.Id.ToString())
                .ToListAsync();

            return unverifies.ConvertAll(o =>
            {
                var user = guild.GetUser(Convert.ToUInt64(o.UserId));
                return ProfileGenerator.Reconstruct(o.UnverifyLog, user, guild);
            });
        }

        public async Task RecoverUnverifyState(long id, ulong fromUserId)
        {
            using var context = DbFactory.Create();

            var logItem = await context.UnverifyLogs.AsNoTracking()
                .Include(o => o.Guild)
                .Include(o => o.ToUser)
                .ThenInclude(o => o.Unverify)
                .FirstOrDefaultAsync(o => o.Id == id && (o.Operation == UnverifyOperation.Selfunverify || o.Operation == UnverifyOperation.Unverify));

            if (logItem == null)
                throw new NotFoundException("Záznam o provedeném odebrání přístupu nebyl nalezen.");

            if (logItem.ToUser.Unverify != null)
                throw new InvalidOperationException("Nelze provést obnovení přístupu uživateli, protože má aktuálně platné unverify.");

            var guild = DiscordClient.GetGuild(Convert.ToUInt64(logItem.GuildId));
            if (guild == null)
                throw new NotFoundException("Nelze najít server, na kterém bylo uděleno unverify.");

            await guild.DownloadUsersAsync();
            var user = guild.GetUser(Convert.ToUInt64(logItem.ToUserId));
            if (user == null)
                throw new NotFoundException($"Nelze vyhledat uživatele na serveru {guild.Name}");

            var mutedRole = !string.IsNullOrEmpty(logItem.Guild.MuteRoleId) ? guild.GetRole(Convert.ToUInt64(logItem.Guild.MuteRoleId)) : null;
            var data = JsonConvert.DeserializeObject<UnverifyLogSet>(logItem.Data);

            var rolesToReturn = data.RolesToRemove.Where(o => !user.Roles.Any(x => x.Id == o))
                .Select(o => guild.GetRole(o) as IRole)
                .Where(role => role != null)
                .ToList();

            var channelsToReturn = data.ChannelsToRemove
                .Select(o => new { Channel = guild.GetChannel(o.ChannelId), Perms = o.Permissions, Obj = o })
                .Where(o => o.Channel != null)
                .Where(o =>
                {
                    var perms = o.Channel.GetPermissionOverwrite(user);
                    return perms != null && (perms.Value.AllowValue != o.Perms.AllowValue || perms.Value.DenyValue != o.Perms.DenyValue);
                })
                .ToList();

            var fromUser = guild.GetUser(fromUserId);
            await Logger.LogRecoverAsync(rolesToReturn, channelsToReturn.ConvertAll(o => o.Obj), guild, fromUser, user, CancellationToken.None);

            if (rolesToReturn.Count > 0)
                await user.AddRolesAsync(rolesToReturn);

            if (channelsToReturn.Count > 0)
            {
                foreach (var channel in channelsToReturn)
                {
                    await channel.Channel.AddPermissionOverwriteAsync(user, channel.Perms);
                }
            }

            if (mutedRole != null)
                await user.RemoveRoleAsync(mutedRole);
        }

        public async Task<List<Tuple<ulong, ulong>>> GetPendingUnverifiesForRemoveAsync(CancellationToken cancellationToken)
        {
            using var context = DbFactory.Create();

            var query = context.Unverifies.AsNoTracking()
                .Where(o => o.EndAt <= DateTime.Now)
                .Select(o => new { o.GuildId, o.UserId });

            return (await query.ToListAsync(cancellationToken)).ConvertAll(o => new Tuple<ulong, ulong>(
                Convert.ToUInt64(o.GuildId),
                Convert.ToUInt64(o.UserId)
            ));
        }
    }
}
