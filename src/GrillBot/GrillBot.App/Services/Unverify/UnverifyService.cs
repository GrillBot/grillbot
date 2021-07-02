using Discord;
using Discord.Net;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Logging;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Services.Unverify
{
    public class UnverifyService : ServiceBase
    {
        private UnverifyChecker Checker { get; }
        private UnverifyProfileGenerator ProfileGenerator { get; }
        private UnverifyLogger Logger { get; }
        private GrillBotContextFactory DbFactory { get; }
        private LoggingService Logging { get; }

        public UnverifyService(DiscordSocketClient client, UnverifyChecker checker, UnverifyProfileGenerator profileGenerator,
            UnverifyLogger logger, GrillBotContextFactory dbFactory, LoggingService logging) : base(client)
        {
            Checker = checker;
            ProfileGenerator = profileGenerator;
            Logger = logger;
            DbFactory = dbFactory;
            Logging = logging;

            DiscordClient.UserLeft += OnUserLeftAsync;
        }

        public async Task<List<string>> SetUnverifyAsync(List<SocketUser> users, DateTime end, string data, SocketGuild guild, IUser from, bool dry)
        {
            await guild.DownloadUsersAsync();
            var messages = new List<string>();

            var muteRole = await GetMutedRoleAsync(guild);
            var fromUser = from as IGuildUser ?? guild.GetUser(from.Id);

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

            await Checker.ValidateUnverifyAsync(guildUser, guild, selfunverify, end);
            var profile = await ProfileGenerator.CreateAsync(guildUser, guild, end, data, selfunverify, keep, muteRole);

            if (dry)
                return UnverifyMessageGenerator.CreateUnverifyMessageToChannel(profile);

            var unverifyLog = await LogUnverifyAsync(profile, guild, from, selfunverify);
            try
            {
                await profile.Destination.TryAddRoleAsync(muteRole);
                await profile.RemoveRolesAsync();
                await profile.ReturnChannelsAsync(guild);

                using var context = DbFactory.Create();

                await context.InitGuildAsync(guild);
                await context.InitUserAsync(from);
                await context.InitGuildUserAsync(guild, from);

                if (from != profile.Destination)
                {
                    await context.InitUserAsync(profile.Destination);
                    await context.InitGuildUserAsync(guild, profile.Destination);
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

        private Task<UnverifyLog> LogUnverifyAsync(UnverifyUserProfile profile, SocketGuild guild, IGuildUser from, bool selfunverify)
        {
            if (selfunverify)
                return Logger.LogSelfunverifyAsync(profile, guild);

            return Logger.LogUnverifyAsync(profile, guild, from);
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

            await Logger.LogUpdateAsync(DateTime.Now, newEnd, guild, fromUser, user);

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

        public async Task<string> RemoveUnverifyAsync(SocketGuild guild, IGuildUser fromUser, IGuildUser toUser, bool isAuto = false)
        {
            try
            {
                using var context = DbFactory.Create();

                var dbUser = await context.GuildUsers
                    .Include(o => o.Unverify)
                    .ThenInclude(o => o.UnverifyLog)
                    .FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.UserId == toUser.Id.ToString());

                if (dbUser?.Unverify == null)
                    return UnverifyMessageGenerator.CreateRemoveAccessUnverifyNotFound(toUser);

                var profile = ProfileGenerator.Reconstruct(dbUser.Unverify.UnverifyLog, toUser, guild);
                await LogRemoveAsync(profile.RolesToRemove.ConvertAll(o => o as IRole), profile.ChannelsToRemove, toUser, guild, fromUser, isAuto);

                var muteRole = await GetMutedRoleAsync(guild);
                if (muteRole != null && profile.Destination.RoleIds.Contains(muteRole.Id))
                    await profile.Destination.RemoveRoleAsync(muteRole);

                await profile.ReturnChannelsAsync(guild);
                await profile.ReturnRolesAsync();

                dbUser.Unverify = null;
                await context.SaveChangesAsync();

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
            IGuildUser fromUser, bool isAuto)
        {
            if (isAuto)
                return Logger.LogAutoremoveAsync(returnedRoles, channels, user, guild);

            return Logger.LogRemoveAsync(returnedRoles, channels, guild, fromUser, user);
        }

        public async Task UnverifyAutoremoveAsync(ulong guildId, ulong userId)
        {
            using var context = DbFactory.Create();

            try
            {
                var unverify = await context.Unverifies.AsQueryable()
                    .FirstOrDefaultAsync(o => o.UserId == userId.ToString() && o.GuildId == guildId.ToString());

                if (unverify == null) return;
                var guild = DiscordClient.GetGuild(guildId);

                if (guild == null)
                {
                    context.Remove(unverify);
                    return;
                }

                var user = guild.GetUser(userId);

                if (user == null)
                {
                    context.Remove(unverify);
                    return;
                }

                await RemoveUnverifyAsync(guild, guild.CurrentUser, user, true);
            }
            catch (Exception ex)
            {
                await Logging.ErrorAsync("Unverify/Autoremove", $"An error occured when unverify returning access ({guildId}/{userId}).", ex);
            }
            finally
            {
                await context.SaveChangesAsync();
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

        // TODO: Selfunverify
    }
}
