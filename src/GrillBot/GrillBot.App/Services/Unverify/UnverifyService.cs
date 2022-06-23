using Discord.Net;
using GrillBot.App.Services.Logging;
using GrillBot.App.Services.Permissions;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Unverify;

public class UnverifyService
{
    private UnverifyChecker Checker { get; }
    private UnverifyProfileGenerator ProfileGenerator { get; }
    private UnverifyLogger Logger { get; }
    private LoggingService Logging { get; }
    private PermissionsCleaner PermissionsCleaner { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UnverifyService(DiscordSocketClient client, UnverifyChecker checker, UnverifyProfileGenerator profileGenerator,
        UnverifyLogger logger, GrillBotDatabaseBuilder databaseBuilder, LoggingService logging, PermissionsCleaner permissionsCleaner)
    {
        Checker = checker;
        ProfileGenerator = profileGenerator;
        Logger = logger;
        Logging = logging;
        PermissionsCleaner = permissionsCleaner;
        DatabaseBuilder = databaseBuilder;
        DiscordClient = client;

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

    public async Task<string> SetUnverifyAsync(IUser user, DateTime end, string data, SocketGuild guild, IGuildUser from, bool selfunverify, List<string> keep,
        IRole muteRole, bool dry)
    {
        if (selfunverify && muteRole == null) muteRole = await GetMutedRoleAsync(guild);
        var guildUser = user as IGuildUser ?? guild.GetUser(user.Id);

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

            await using var repository = DatabaseBuilder.CreateRepository();

            await repository.GuildUser.GetOrCreateGuildUserAsync(from);
            var dbGuildUser = await repository.GuildUser.GetOrCreateGuildUserAsync(profile.Destination);

            dbGuildUser.Unverify = new Database.Entity.Unverify
            {
                Channels = profile.ChannelsToRemove.ConvertAll(o => new GuildChannelOverride { AllowValue = o.AllowValue, ChannelId = o.ChannelId, DenyValue = o.DenyValue }),
                EndAt = profile.End,
                Guild = await repository.Guild.GetOrCreateRepositoryAsync(guild),
                Reason = profile.Reason,
                Roles = profile.RolesToRemove.ConvertAll(o => o.Id.ToString()),
                SetOperationId = unverifyLog.Id,
                StartAt = profile.Start
            };

            await repository.CommitAsync();

            try
            {
                var dmMessage = UnverifyMessageGenerator.CreateUnverifyPmMessage(profile, guild);
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
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildData = await repository.Guild.FindGuildAsync(guild, true);
        return string.IsNullOrEmpty(guildData?.MuteRoleId) ? null : guild.GetRole(guildData.MuteRoleId.ToUlong());
    }

    private Task<UnverifyLog> LogUnverifyAsync(UnverifyUserProfile profile, SocketGuild guild, IGuildUser from, bool selfunverify)
    {
        return selfunverify ? Logger.LogSelfunverifyAsync(profile, guild) : Logger.LogUnverifyAsync(profile, guild, from);
    }

    public async Task<string> UpdateUnverifyAsync(IGuildUser user, IGuild guild, DateTime newEnd, IGuildUser fromUser)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbUser = await repository.GuildUser.FindGuildUserAsync(user);
        if (dbUser?.Unverify == null)
            throw new NotFoundException("Aktualizaci času nelze pro hledaného uživatele provést. Unverify nenalezeno.");

        if ((dbUser.Unverify.EndAt - DateTime.Now).TotalSeconds <= 30.0)
            throw new ValidationException("Aktualizace data a času již není možná. Vypršel čas nebo zbývá méně než půl minuty.");

        await Logger.LogUpdateAsync(DateTime.Now, newEnd, guild, fromUser, user);

        dbUser.Unverify.EndAt = newEnd;
        dbUser.Unverify.StartAt = DateTime.Now;
        await repository.CommitAsync();

        try
        {
            var dmMessage = UnverifyMessageGenerator.CreateUpdatePmMessage(guild, newEnd);
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
            await using var repository = DatabaseBuilder.CreateRepository();

            var dbUser = await repository.GuildUser.FindGuildUserAsync(toUser);
            if (dbUser?.Unverify == null)
                return UnverifyMessageGenerator.CreateRemoveAccessUnverifyNotFound(toUser);

            var profile = UnverifyProfileGenerator.Reconstruct(dbUser.Unverify, toUser, guild);
            await LogRemoveAsync(profile.RolesToRemove, profile.ChannelsToRemove, toUser, guild, fromUser, isAuto);

            var muteRole = await GetMutedRoleAsync(guild);
            if (muteRole != null && profile.Destination.RoleIds.Contains(muteRole.Id))
                await profile.Destination.RemoveRoleAsync(muteRole);

            await profile.ReturnChannelsAsync(guild);
            await profile.ReturnRolesAsync();

            dbUser.Unverify = null;
            await repository.CommitAsync();

            var uselessPermissions = await PermissionsCleaner.GetUselessPermissionsForUser(toUser, guild);
            if (uselessPermissions.Count > 0)
            {
                foreach (var permission in uselessPermissions)
                    await PermissionsCleaner.RemoveUselessPermissionAsync(permission);
            }

            if (isAuto)
                return UnverifyMessageGenerator.CreateRemoveAccessManuallyToChannel(toUser);

            try
            {
                var dmMessage = UnverifyMessageGenerator.CreateRemoveAccessManuallyPmMessage(guild);
                await toUser.SendMessageAsync(dmMessage);
            }
            catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
            {
                // User have disabled DMs.
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
        return isAuto ? Logger.LogAutoremoveAsync(returnedRoles, channels, user, guild) : Logger.LogRemoveAsync(returnedRoles, channels, guild, fromUser, user);
    }

    public async Task UnverifyAutoremoveAsync(ulong guildId, ulong userId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        try
        {
            var unverify = await repository.Unverify.FindUnverifyAsync(guildId, userId);
            if (unverify == null) return;

            var guild = DiscordClient.GetGuild(guildId);
            if (guild == null)
            {
                repository.Remove(unverify);
                return;
            }

            await guild.DownloadUsersAsync();
            var user = guild.GetUser(userId);

            if (user == null)
            {
                repository.Remove(unverify);
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
            await repository.CommitAsync();
        }
    }

    private async Task OnUserLeftAsync(IGuild guild, IUser user)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildUser = user as IGuildUser ?? await guild.GetUserAsync(user.Id);
        if (guildUser == null) return;

        var dbUser = await repository.GuildUser.FindGuildUserAsync(guildUser);
        if (dbUser?.Unverify != null)
        {
            dbUser.Unverify = null;
            await repository.CommitAsync();
        }
    }

    public async Task<int> GetUnverifyCountsOfGuildAsync(IGuild guild)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.Unverify.GetUnverifyCountsAsync(guild);
    }

    public async Task<UnverifyUserProfile> GetCurrentUnverifyAsync(IGuild guild, int page)
    {
        await guild.DownloadUsersAsync();

        await using var repository = DatabaseBuilder.CreateRepository();

        var unverify = await repository.Unverify.FindUnverifyPageAsync(guild, page);
        if (unverify == null)
            return null;

        var user = await guild.GetUserAsync(unverify.UserId.ToUlong());
        var profile = UnverifyProfileGenerator.Reconstruct(unverify, user, guild);

        var hiddenChannelsData = await repository.Channel.GetAllChannelsAsync(new List<string> { guild.Id.ToString() }, true, true);
        var hiddenChannels = hiddenChannelsData
            .Where(o => o.HasFlag(ChannelFlags.StatsHidden))
            .Select(o => o.ChannelId.ToUlong())
            .ToList();

        profile.ChannelsToKeep = profile.ChannelsToKeep.FindAll(o => !hiddenChannels.Contains(o.ChannelId));
        profile.ChannelsToRemove = profile.ChannelsToRemove.FindAll(o => !hiddenChannels.Contains(o.ChannelId));

        return profile;
    }

    public async Task<List<(UnverifyUserProfile profile, IGuild guild)>> GetAllUnverifiesAsync(ulong? userId = null)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var unverifies = await repository.Unverify.GetUnverifiesAsync(userId);

        var profiles = new List<(UnverifyUserProfile profile, IGuild guild)>();
        foreach (var unverify in unverifies)
        {
            if (DiscordClient.GetGuild(unverify.GuildId.ToUlong()) is not IGuild guild) continue;

            var user = await guild.GetUserAsync(unverify.UserId.ToUlong());
            var profile = UnverifyProfileGenerator.Reconstruct(unverify, user, guild);

            profiles.Add((profile, guild));
        }

        return profiles;
    }

    public async Task<List<ulong>> GetUserIdsWithUnverifyAsync(IGuild guild)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.Unverify.GetUserIdsWithUnverify(guild);
    }

    public async Task RecoverUnverifyState(long id, ulong fromUserId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var logItem = await repository.Unverify.FindUnverifyLogByIdAsync(id);

        if (logItem == null || (logItem.Operation != UnverifyOperation.Selfunverify && logItem.Operation != UnverifyOperation.Unverify))
            throw new NotFoundException("Záznam o provedeném odebrání přístupu nebyl nalezen.");

        if (logItem.ToUser!.Unverify != null)
            throw new InvalidOperationException("Nelze provést obnovení přístupu uživateli, protože má aktuálně platné unverify.");

        var guild = DiscordClient.GetGuild(logItem.GuildId.ToUlong());
        if (guild == null)
            throw new NotFoundException("Nelze najít server, na kterém bylo uděleno unverify.");

        await guild.DownloadUsersAsync();
        var user = guild.GetUser(logItem.ToUserId.ToUlong());
        if (user == null)
            throw new NotFoundException($"Nelze vyhledat uživatele na serveru {guild.Name}");

        var mutedRole = !string.IsNullOrEmpty(logItem.Guild!.MuteRoleId) ? guild.GetRole(logItem.Guild.MuteRoleId.ToUlong()) : null;
        var data = JsonConvert.DeserializeObject<UnverifyLogSet>(logItem.Data);
        if (data == null)
            throw new GrillBotException("Nepodařilo se zpracovat data logu unverify.");

        var rolesToReturn = data.RolesToRemove
            .Where(o => user.Roles.All(x => x.Id != o))
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
                await channel.Channel.AddPermissionOverwriteAsync(user, channel.Perms);
        }

        if (mutedRole != null)
            await user.RemoveRoleAsync(mutedRole);
    }

    public async Task<List<(ulong guildId, ulong userId)>> GetPendingUnverifiesForRemoveAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.Unverify.GetPendingUnverifyIdsAsync();
    }
}
