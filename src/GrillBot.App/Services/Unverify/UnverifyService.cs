using Discord.Net;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Logging;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Unverify;

public class UnverifyService
{
    private UnverifyChecker Checker { get; }
    private UnverifyProfileGenerator ProfileGenerator { get; }
    private UnverifyLogManager LogManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private LoggingManager LoggingManager { get; }
    private UnverifyMessageGenerator MessageGenerator { get; }
    private UnverifyHelper UnverifyHelper { get; }

    public UnverifyService(UnverifyChecker checker, UnverifyProfileGenerator profileGenerator, UnverifyLogManager logManager, GrillBotDatabaseBuilder databaseBuilder, LoggingManager loggingManager,
        UnverifyMessageGenerator messageGenerator)
    {
        Checker = checker;
        ProfileGenerator = profileGenerator;
        LogManager = logManager;
        DatabaseBuilder = databaseBuilder;
        LoggingManager = loggingManager;
        MessageGenerator = messageGenerator;
        UnverifyHelper = new UnverifyHelper(databaseBuilder);
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
        return selfunverify ? LogManager.LogSelfunverifyAsync(profile, guild) : LogManager.LogUnverifyAsync(profile, guild, from);
    }
}
