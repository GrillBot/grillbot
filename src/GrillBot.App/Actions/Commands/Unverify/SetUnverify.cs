using Discord.Net;
using GrillBot.App.Helpers;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Logging;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;

namespace GrillBot.App.Actions.Commands.Unverify;

public class SetUnverify : CommandAction
{
    private UnverifyHelper UnverifyHelper { get; }
    private UnverifyCheckManager CheckManager { get; }
    private UnverifyProfileManager ProfileManager { get; }
    private UnverifyMessageManager MessageManager { get; }
    private UnverifyLogManager LogManager { get; }
    private LoggingManager LoggingManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private IRole? MutedRole { get; set; }
    private IGuildUser? ExecutingUser { get; set; }

    public SetUnverify(UnverifyHelper unverifyHelper, UnverifyCheckManager checkManager, UnverifyProfileManager profileManager, UnverifyMessageManager messageManager, UnverifyLogManager logManager,
        LoggingManager loggingManager, GrillBotDatabaseBuilder databaseBuilder)
    {
        UnverifyHelper = unverifyHelper;
        CheckManager = checkManager;
        ProfileManager = profileManager;
        MessageManager = messageManager;
        LogManager = logManager;
        LoggingManager = loggingManager;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<List<string>> ProcessAsync(List<IGuildUser> users, DateTime end, string reason, bool testRun)
    {
        var messages = new List<string>();
        foreach (var user in users)
        {
            var message = await ProcessAsync(user, end, reason, false, new List<string>(), testRun);
            messages.Add(message);
        }

        return messages;
    }

    public async Task<string> ProcessAsync(IUser user, DateTime end, string? reason, bool selfUnverify, List<string> toKeep, bool testRun)
    {
        var guildUser = user as IGuildUser ?? await Context.Guild.GetUserAsync(user.Id);
        await CheckManager.ValidateUnverifyAsync(guildUser, Context.Guild, selfUnverify, end, toKeep.Count, Locale);

        await InitAsync();

        var userLanguage = await UnverifyHelper.GetUserLanguageAsync(guildUser, Locale, selfUnverify);
        var profile = await ProfileManager.CreateAsync(guildUser, Context.Guild, end, reason, selfUnverify, toKeep, MutedRole, userLanguage, Locale);
        if (testRun)
            return MessageManager.CreateUnverifyMessageToChannel(profile, Locale);

        try
        {
            var logItem = await LogUnverifyAsync(profile, selfUnverify);

            await ProcessAsync(profile, logItem);
            await SendSuccessRemovalPmAsync(profile);
            return MessageManager.CreateUnverifyMessageToChannel(profile, Locale);
        }
        catch (Exception ex)
        {
            await LoggingManager.ErrorAsync("Unverify", $"An error occured while removing access to {user.GetFullName()}", ex);
            await ReverseAsync(profile);
            return MessageManager.CreateUnverifyFailedToChannel(profile.Destination, Locale);
        }
    }

    private async Task InitAsync()
    {
        MutedRole ??= await UnverifyHelper.GetMuteRoleAsync(Context.Guild);
        ExecutingUser ??= await GetExecutingUserAsync();
    }

    private Task<UnverifyLog> LogUnverifyAsync(UnverifyUserProfile profile, bool selfunverify)
    {
        return selfunverify ? LogManager.LogSelfunverifyAsync(profile, Context.Guild) : LogManager.LogUnverifyAsync(profile, Context.Guild, ExecutingUser!);
    }

    private async Task ProcessAsync(UnverifyUserProfile profile, UnverifyLog logItem)
    {
        if (MutedRole != null)
            await profile.Destination.TryAddRoleAsync(MutedRole);

        await profile.RemoveRolesAsync();
        await profile.RemoveChannelsAsync(Context.Guild);

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(Context.Guild);
        await repository.User.GetOrCreateUserAsync(ExecutingUser!);
        await repository.GuildUser.GetOrCreateGuildUserAsync(ExecutingUser!);
        await repository.User.GetOrCreateUserAsync(profile.Destination);

        var guildUserEntity = await repository.GuildUser.GetOrCreateGuildUserAsync(profile.Destination, true);
        guildUserEntity.Unverify = profile.CreateRecord(Context.Guild, logItem.Id);

        await repository.CommitAsync();
    }

    private async Task SendSuccessRemovalPmAsync(UnverifyUserProfile profile)
    {
        try
        {
            var message = MessageManager.CreateUpdatePmMessage(Context.Guild, profile.End, profile.Reason, Locale);
            await profile.Destination.SendMessageAsync(message);
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            // User have disabled DMs.
        }
    }

    private async Task ReverseAsync(UnverifyUserProfile profile)
    {
        await profile.ReturnRolesAsync();
        await profile.ReturnChannelsAsync(Context.Guild);

        if (!profile.KeepMutedRole && MutedRole != null)
            await profile.Destination.TryRemoveRoleAsync(MutedRole);
    }
}
