using Discord.Net;
using GrillBot.App.Services.Unverify;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class RemoveUnverify : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private UnverifyMessageGenerator MessageGenerator { get; }
    private UnverifyLogger UnverifyLogger { get; }
    private LoggingManager LoggingManager { get; }
    private UnverifyHelper UnverifyHelper { get; }

    private bool IsAutoRemove { get; set; }
    private bool IsForceRemove { get; set; }

    public RemoveUnverify(ApiRequestContext apiContext, IDiscordClient discordClient, ITextsManager texts, GrillBotDatabaseBuilder databaseBuilder, UnverifyMessageGenerator messageGenerator,
        UnverifyLogger unverifyLogger, LoggingManager loggingManager, UnverifyHelper unverifyHelper) : base(apiContext)
    {
        DiscordClient = discordClient;
        Texts = texts;
        DatabaseBuilder = databaseBuilder;
        MessageGenerator = messageGenerator;
        UnverifyLogger = unverifyLogger;
        LoggingManager = loggingManager;
        UnverifyHelper = unverifyHelper;
    }

    public async Task ProcessAutoRemoveAsync(ulong guildId, ulong userId)
    {
        try
        {
            IsAutoRemove = true;
            await ProcessAsync(guildId, userId);
        }
        catch (NotFoundException)
        {
            // There is not reason why throw exception if removal process is from job.  
            await ForceRemoveAsync(guildId, userId);
        }
    }

    public async Task<string> ProcessAsync(ulong guildId, ulong userId, bool force = false)
    {
        IsForceRemove = force;

        var (guild, toUser, fromUser) = await InitAsync(guildId, userId);
        try
        {
            return await ProcessAsync(guild, fromUser, toUser);
        }
        catch (Exception ex) when (!IsAutoRemove)
        {
            await LoggingManager.ErrorAsync(nameof(RemoveUnverify), "An error occured when unverify returning access.", ex);
            return MessageGenerator.CreateRemoveAccessManuallyFailed(toUser, ex, "cs");
        }
    }

    private async Task<(IGuild guild, IGuildUser toUser, IGuildUser fromUser)> InitAsync(ulong guildId, ulong userId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId);
        var toUser = guild == null ? null : await guild.GetUserAsync(userId);
        var fromUser = guild == null ? null : await guild.GetUserAsync(ApiContext.GetUserId());

        ValidateData(guild, toUser);
        return (guild, toUser, fromUser);
    }

    private void ValidateData(IGuild guild, IGuildUser toUser)
    {
        if (guild == null)
            throw new NotFoundException(Texts["Unverify/GuildNotFound", ApiContext.Language]);
        if (toUser == null)
            throw new NotFoundException(Texts["Unverify/DestUserNotFound", ApiContext.Language]);
    }

    private async Task ForceRemoveAsync(ulong guildId, ulong userId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var unverify = await repository.Unverify.FindUnverifyAsync(guildId, userId);
        if (unverify != null)
        {
            repository.Remove(unverify);
            await repository.CommitAsync();
        }
    }

    private async Task<string> ProcessAsync(IGuild guild, IGuildUser fromUser, IGuildUser toUser)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var unverify = await repository.Unverify.FindUnverifyAsync(guild.Id, toUser.Id, includeLogs: true);
        if (unverify == null)
            return MessageGenerator.CreateRemoveAccessUnverifyNotFound(toUser, ApiContext.Language);

        var profile = UnverifyProfileGenerator.Reconstruct(unverify, toUser, guild);
        await WriteToLogAsync(profile.RolesToRemove, profile.ChannelsToRemove, fromUser, toUser, guild, profile.Language);

        if (!IsForceRemove)
        {
            var muteRole = await UnverifyHelper.GetMuteRoleAsync(guild);
            if (muteRole != null && profile.Destination.RoleIds.Contains(muteRole.Id))
                await profile.Destination.RemoveRoleAsync(muteRole);

            await profile.ReturnChannelsAsync(guild);
            await profile.ReturnRolesAsync();
        }

        repository.Remove(unverify);
        await repository.CommitAsync();

        if (IsAutoRemove)
            return MessageGenerator.CreateRemoveAccessManuallyToChannel(toUser, ApiContext.Language);

        try
        {
            var dmMessage = MessageGenerator.CreateRemoveAccessManuallyPmMessage(guild, profile.Language);
            await toUser.SendMessageAsync(dmMessage);
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            // User have disabled DMs.
        }

        return MessageGenerator.CreateRemoveAccessManuallyToChannel(toUser, ApiContext.Language);
    }

    private async Task WriteToLogAsync(List<IRole> roles, List<ChannelOverride> channels, IGuildUser fromUser, IGuildUser toUser, IGuild guild, string userLanguage)
    {
        if (IsAutoRemove)
        {
            await UnverifyLogger.LogAutoremoveAsync(roles, channels, toUser, guild, userLanguage);
        }
        else
        {
            if (IsForceRemove)
                await UnverifyLogger.LogRemoveAsync(new List<IRole>(), new List<ChannelOverride>(), guild, fromUser, toUser, IsApiRequest, true, userLanguage);
            else
                await UnverifyLogger.LogRemoveAsync(roles, channels, guild, fromUser, toUser, IsApiRequest, IsForceRemove, userLanguage);
        }
    }
}
