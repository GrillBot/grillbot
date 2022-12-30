using Discord.Net;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Managers.Logging;
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
    private DiscordSocketClient DiscordSocketClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private LoggingManager LoggingManager { get; }
    private ITextsManager Texts { get; }
    private UnverifyMessageGenerator MessageGenerator { get; }
    private IDiscordClient DiscordClient { get; }
    private UnverifyHelper UnverifyHelper { get; }

    public UnverifyService(DiscordSocketClient discordSocketClient, UnverifyChecker checker, UnverifyProfileGenerator profileGenerator, UnverifyLogger logger, GrillBotDatabaseBuilder databaseBuilder,
        LoggingManager loggingManager, ITextsManager texts, UnverifyMessageGenerator messageGenerator, IDiscordClient discordClient)
    {
        Checker = checker;
        ProfileGenerator = profileGenerator;
        Logger = logger;
        DatabaseBuilder = databaseBuilder;
        DiscordSocketClient = discordSocketClient;
        LoggingManager = loggingManager;
        Texts = texts;
        MessageGenerator = messageGenerator;
        DiscordClient = discordClient;
        UnverifyHelper = new UnverifyHelper(databaseBuilder);

        DiscordSocketClient.UserLeft += OnUserLeftAsync;
    }

    public async Task<List<string>> SetUnverifyAsync(List<IGuildUser> users, DateTime end, string data, IGuild guild, IUser from, bool dry, string locale)
    {
        var muteRole = await GetMutedRoleAsync(guild);
        var fromUser = from as IGuildUser ?? await guild.GetUserAsync(from.Id);

        var messages = new List<string>();
        foreach (var user in users)
        {
            var message = await SetUnverifyAsync(user, end, data, guild, fromUser, false, new List<string>(), muteRole, dry, locale);
            messages.Add(message);
        }

        return messages;
    }

    public async Task<string> SetUnverifyAsync(IUser user, DateTime end, string data, IGuild guild, IGuildUser from, bool selfunverify, List<string> keep,
        IRole muteRole, bool dry, string locale)
    {
        if (selfunverify && muteRole == null) muteRole = await GetMutedRoleAsync(guild);
        var guildUser = user as IGuildUser ?? await guild.GetUserAsync(user.Id);

        await Checker.ValidateUnverifyAsync(guildUser, guild, selfunverify, end, keep?.Count ?? 0, locale);

        var userLanguage = await UnverifyHelper.GetUserLanguageAsync(guildUser, locale, selfunverify);
        var profile = await ProfileGenerator.CreateAsync(guildUser, guild, end, data, selfunverify, keep, muteRole, userLanguage, locale);

        if (dry)
            return MessageGenerator.CreateUnverifyMessageToChannel(profile, locale);

        var unverifyLog = await LogUnverifyAsync(profile, guild, from, selfunverify);
        try
        {
            await profile.Destination.TryAddRoleAsync(muteRole);
            await profile.RemoveRolesAsync();
            await profile.RemoveChannelsAsync(guild);

            await using var repository = DatabaseBuilder.CreateRepository();

            await repository.Guild.GetOrCreateGuildAsync(guild);
            await repository.User.GetOrCreateUserAsync(from);
            await repository.GuildUser.GetOrCreateGuildUserAsync(from);
            await repository.User.GetOrCreateUserAsync(profile.Destination);
            var dbGuildUser = await repository.GuildUser.GetOrCreateGuildUserAsync(profile.Destination, true);

            dbGuildUser.Unverify = new Database.Entity.Unverify
            {
                Channels = profile.ChannelsToRemove.ConvertAll(o => new GuildChannelOverride { AllowValue = o.AllowValue, ChannelId = o.ChannelId, DenyValue = o.DenyValue }),
                EndAt = profile.End,
                Guild = await repository.Guild.GetOrCreateGuildAsync(guild),
                Reason = profile.Reason,
                Roles = profile.RolesToRemove.ConvertAll(o => o.Id.ToString()),
                SetOperationId = unverifyLog.Id,
                StartAt = profile.Start
            };

            await repository.CommitAsync();

            try
            {
                var dmMessage = MessageGenerator.CreateUnverifyPmMessage(profile, guild, userLanguage);
                await profile.Destination.SendMessageAsync(dmMessage);
            }
            catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
            {
                // User have disabled DMs.
            }

            return MessageGenerator.CreateUnverifyMessageToChannel(profile, locale);
        }
        catch (Exception ex)
        {
            await profile.Destination.TryRemoveRoleAsync(muteRole);
            await profile.ReturnRolesAsync();
            await profile.ReturnChannelsAsync(guild);
            await LoggingManager.ErrorAsync("Unverify", $"An error occured when unverify removing access to {user.GetFullName()}", ex);
            return MessageGenerator.CreateUnverifyFailedToChannel(profile.Destination, locale);
        }
    }

    private async Task<IRole> GetMutedRoleAsync(IGuild guild)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildData = await repository.Guild.FindGuildAsync(guild, true);
        return string.IsNullOrEmpty(guildData?.MuteRoleId) ? null : guild.GetRole(guildData.MuteRoleId.ToUlong());
    }

    private Task<UnverifyLog> LogUnverifyAsync(UnverifyUserProfile profile, IGuild guild, IGuildUser from, bool selfunverify)
    {
        return selfunverify ? Logger.LogSelfunverifyAsync(profile, guild) : Logger.LogUnverifyAsync(profile, guild, from);
    }

    private async Task OnUserLeftAsync(IGuild guild, IUser user)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildUser = user as IGuildUser ?? await guild.GetUserAsync(user.Id);
        if (guildUser == null) return;

        var dbUser = await repository.GuildUser.FindGuildUserAsync(guildUser, includeAll: true);
        if (dbUser?.Unverify != null)
        {
            dbUser.Unverify = null;
            await repository.CommitAsync();
        }
    }

    public async Task<List<(UnverifyUserProfile profile, IGuild guild)>> GetAllUnverifiesAsync(ulong? userId = null)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var unverifies = await repository.Unverify.GetUnverifiesAsync(userId);

        var profiles = new List<(UnverifyUserProfile profile, IGuild guild)>();
        foreach (var unverify in unverifies)
        {
            var guild = await DiscordClient.GetGuildAsync(unverify.GuildId.ToUlong());
            if (guild is null) continue;

            var user = await guild.GetUserAsync(unverify.UserId.ToUlong());
            var profile = UnverifyProfileGenerator.Reconstruct(unverify, user, guild);

            profiles.Add((profile, guild));
        }

        return profiles;
    }

    public async Task RecoverUnverifyState(long id, ulong fromUserId, string locale)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var logItem = await repository.Unverify.FindUnverifyLogByIdAsync(id);

        if (logItem == null || (logItem.Operation != UnverifyOperation.Selfunverify && logItem.Operation != UnverifyOperation.Unverify))
            throw new NotFoundException(Texts["Unverify/Recover/LogItemNotFound", locale]);

        if (logItem.ToUser!.Unverify != null)
            throw new ValidationException(Texts["Unverify/Recover/ValidUnverify", locale]).ToBadRequestValidation(id, nameof(logItem.ToUser.Unverify));

        var guild = await DiscordClient.GetGuildAsync(logItem.GuildId.ToUlong());
        if (guild == null)
            throw new NotFoundException(Texts["Unverify/Recover/GuildNotFound", locale]);

        var user = await guild.GetUserAsync(logItem.ToUserId.ToUlong());
        if (user == null)
            throw new NotFoundException(Texts["Unverify/Recover/MemberNotFound", locale].FormatWith(guild.Name));

        var mutedRole = !string.IsNullOrEmpty(logItem.Guild!.MuteRoleId) ? guild.GetRole(logItem.Guild.MuteRoleId.ToUlong()) : null;
        var data = JsonConvert.DeserializeObject<UnverifyLogSet>(logItem.Data)!;

        var rolesToReturn = data.RolesToRemove
            .Where(o => user.RoleIds.All(x => x != o))
            .Select(o => guild.GetRole(o))
            .Where(role => role != null)
            .ToList();

        var channelsToReturn = new List<(IGuildChannel channel, OverwritePermissions permissions, ChannelOverride @override)>();
        foreach (var item in data.ChannelsToRemove)
        {
            var channel = await guild.GetChannelAsync(item.ChannelId);
            var perms = channel?.GetPermissionOverwrite(user);
            if (perms == null || (perms.Value.AllowValue == item.Permissions.AllowValue && perms.Value.DenyValue == item.Permissions.DenyValue)) continue;

            channelsToReturn.Add((channel, item.Permissions, item));
        }

        var fromUser = await guild.GetUserAsync(fromUserId);
        await Logger.LogRecoverAsync(rolesToReturn, channelsToReturn.ConvertAll(o => o.@override), guild, fromUser, user);

        if (rolesToReturn.Count > 0)
            await user.AddRolesAsync(rolesToReturn);

        foreach (var channel in channelsToReturn)
            await channel.channel.AddPermissionOverwriteAsync(user, channel.permissions);

        if (mutedRole != null)
            await user.RemoveRoleAsync(mutedRole);
    }
}
