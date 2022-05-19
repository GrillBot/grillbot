using Discord.Net;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Logging;
using GrillBot.App.Services.Permissions;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Extensions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Unverify;

public class UnverifyService : ServiceBase
{
    private UnverifyChecker Checker { get; }
    private UnverifyProfileGenerator ProfileGenerator { get; }
    private UnverifyLogger Logger { get; }
    private LoggingService Logging { get; }
    private PermissionsCleaner PermissionsCleaner { get; }

    public UnverifyService(DiscordSocketClient client, UnverifyChecker checker, UnverifyProfileGenerator profileGenerator,
        UnverifyLogger logger, GrillBotContextFactory dbFactory, LoggingService logging, PermissionsCleaner permissionsCleaner) : base(client, dbFactory)
    {
        Checker = checker;
        ProfileGenerator = profileGenerator;
        Logger = logger;
        Logging = logging;
        PermissionsCleaner = permissionsCleaner;

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

        await Checker.ValidateUnverifyAsync(guildUser, guild, selfunverify, end, keep?.Count ?? 0);
        var profile = await ProfileGenerator.CreateAsync(guildUser, guild, end, data, selfunverify, keep, muteRole);

        if (dry)
            return UnverifyMessageGenerator.CreateUnverifyMessageToChannel(profile);

        var unverifyLog = await LogUnverifyAsync(profile, guild, from, selfunverify);
        try
        {
            await profile.Destination.TryAddRoleAsync(muteRole);
            await profile.RemoveRolesAsync();
            await profile.RemoveChannelsAsync(guild);

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
            catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
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
        return string.IsNullOrEmpty(dbGuild?.MuteRoleId) ? null : guild.GetRole(dbGuild.MuteRoleId.ToUlong());
    }

    private Task<UnverifyLog> LogUnverifyAsync(UnverifyUserProfile profile, SocketGuild guild, IGuildUser from, bool selfunverify)
    {
        if (selfunverify)
            return Logger.LogSelfunverifyAsync(profile, guild);

        return Logger.LogUnverifyAsync(profile, guild, from);
    }

    public async Task<string> UpdateUnverifyAsync(IGuildUser user, IGuild guild, DateTime newEnd, IGuildUser fromUser)
    {
        using var context = DbFactory.Create();

        var dbUser = await context.GuildUsers
            .Include(o => o.Unverify)
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
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            // User have disabled DMs.
        }

        return UnverifyMessageGenerator.CreateUpdateChannelMessage(user, newEnd);
    }

    public async Task<string> RemoveUnverifyAsync(IGuild guild, IGuildUser fromUser, IGuildUser toUser, bool isAuto = false)
    {
        try
        {
            using var context = DbFactory.Create();

            var dbUser = await context.GuildUsers
                .Include(o => o.Unverify.UnverifyLog)
                .FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.UserId == toUser.Id.ToString());

            if (dbUser?.Unverify == null)
                return UnverifyMessageGenerator.CreateRemoveAccessUnverifyNotFound(toUser);

            var profile = ProfileGenerator.Reconstruct(dbUser.Unverify, toUser, guild);
            await LogRemoveAsync(profile.RolesToRemove, profile.ChannelsToRemove, toUser, guild, fromUser, isAuto);

            var muteRole = await GetMutedRoleAsync(guild);
            if (muteRole != null && profile.Destination.RoleIds.Contains(muteRole.Id))
                await profile.Destination.RemoveRoleAsync(muteRole);

            await profile.ReturnChannelsAsync(guild);
            await profile.ReturnRolesAsync();

            dbUser.Unverify = null;
            await context.SaveChangesAsync();

            var uselessPermissions = await PermissionsCleaner.GetUselessPermissionsForUser(toUser, guild);
            if (uselessPermissions.Count > 0)
            {
                foreach (var permission in uselessPermissions)
                    await permission.Channel.RemovePermissionOverwriteAsync(toUser);
            }

            if (!isAuto)
            {
                try
                {
                    var dmMessage = UnverifyMessageGenerator.CreateRemoveAccessManuallyPMMessage(guild);
                    await toUser.SendMessageAsync(dmMessage);
                }
                catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
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

            await guild.DownloadUsersAsync();
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

    public async Task OnUserLeftAsync(SocketGuild guild, SocketUser user)
    {
        using var context = DbFactory.Create();

        var unverify = await context.Unverifies.AsQueryable()
            .FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.UserId == user.Id.ToString());

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

        var unverify = await context.Unverifies.AsNoTracking()
            .Include(o => o.UnverifyLog)
            .Where(o => o.GuildId == guild.Id.ToString())
            .OrderBy(o => o.StartAt)
            .Skip(page)
            .FirstOrDefaultAsync();

        if (unverify == null)
            return null;

        var user = guild.GetUser(unverify.UserId.ToUlong());
        var profile = ProfileGenerator.Reconstruct(unverify, user, guild);

        var hiddenChannels = await context.Channels.AsNoTracking()
            .Where(o => o.GuildId == guild.Id.ToString() && (o.Flags & (long)ChannelFlags.StatsHidden) != 0 && (o.Flags & (long)ChannelFlags.Deleted) == 0)
            .Select(o => o.ChannelId)
            .ToListAsync();

        profile.ChannelsToKeep = profile.ChannelsToKeep.FindAll(o => !hiddenChannels.Contains(o.ChannelId.ToString()));
        profile.ChannelsToRemove = profile.ChannelsToRemove.FindAll(o => !hiddenChannels.Contains(o.ChannelId.ToString()));

        return profile;
    }

    public async Task<List<Tuple<UnverifyUserProfile, IGuild>>> GetAllUnverifiesAsync(ulong? userId = null, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var unverifyQuery = context.Unverifies.AsNoTracking()
            .Include(o => o.UnverifyLog)
            .AsQueryable();

        if (userId != null)
            unverifyQuery = unverifyQuery.Where(o => o.UserId == userId.Value.ToString());

        var unverifies = await unverifyQuery.ToListAsync(cancellationToken);
        var profiles = new List<Tuple<UnverifyUserProfile, IGuild>>();
        foreach (var unverify in unverifies)
        {
            var guild = DiscordClient.GetGuild(unverify.GuildId.ToUlong());
            if (guild == null) continue;

            var user = guild.GetUser(unverify.UserId.ToUlong());
            profiles.Add(new Tuple<UnverifyUserProfile, IGuild>(
                ProfileGenerator.Reconstruct(unverify, user, guild),
                guild
            ));
        }

        return profiles;
    }

    public async Task<List<ulong>> GetUserIdsWithUnverifyAsync(IGuild guild)
    {
        using var context = DbFactory.Create();

        var data = await context.Unverifies.AsNoTracking()
            .Where(o => o.GuildId == guild.Id.ToString())
            .Select(o => o.UserId)
            .ToListAsync();

        return data.ConvertAll(o => o.ToUlong());
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

        var guild = DiscordClient.GetGuild(logItem.GuildId.ToUlong());
        if (guild == null)
            throw new NotFoundException("Nelze najít server, na kterém bylo uděleno unverify.");

        await guild.DownloadUsersAsync();
        var user = guild.GetUser(logItem.ToUserId.ToUlong());
        if (user == null)
            throw new NotFoundException($"Nelze vyhledat uživatele na serveru {guild.Name}");

        var mutedRole = !string.IsNullOrEmpty(logItem.Guild.MuteRoleId) ? guild.GetRole(logItem.Guild.MuteRoleId.ToUlong()) : null;
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
        await Logger.LogRecoverAsync(rolesToReturn, channelsToReturn.ConvertAll(o => o.Obj), guild, fromUser, user);

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
            o.GuildId.ToUlong(),
            o.UserId.ToUlong()
        ));
    }
}
