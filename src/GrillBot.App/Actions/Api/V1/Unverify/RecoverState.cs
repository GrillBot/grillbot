using GrillBot.App.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class RecoverState : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private IDiscordClient DiscordClient { get; }
    private UnverifyLogManager LogManager { get; }

    public RecoverState(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, IDiscordClient discordClient, UnverifyLogManager logManager) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        DiscordClient = discordClient;
        LogManager = logManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var logId = (long)Parameters[0]!;

        await using var repository = DatabaseBuilder.CreateRepository();
        var logItem = await repository.Unverify.FindUnverifyLogByIdAsync(logId);

        if (logItem == null || (logItem.Operation != UnverifyOperation.Selfunverify && logItem.Operation != UnverifyOperation.Unverify))
            throw new NotFoundException(Texts["Unverify/Recover/LogItemNotFound", ApiContext.Language]);

        if (logItem.ToUser!.Unverify != null)
            throw new ValidationException(Texts["Unverify/Recover/ValidUnverify", ApiContext.Language]).ToBadRequestValidation(logId, nameof(logItem.ToUser.Unverify));

        var guild = await DiscordClient.GetGuildAsync(logItem.GuildId.ToUlong())
            ?? throw new NotFoundException(Texts["Unverify/Recover/GuildNotFound", ApiContext.Language]);

        var user = await guild.GetUserAsync(logItem.ToUserId.ToUlong())
            ?? throw new NotFoundException(Texts["Unverify/Recover/MemberNotFound", ApiContext.Language].FormatWith(guild.Name));

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

            channelsToReturn.Add((channel!, item.Permissions, item));
        }

        var fromUser = await guild.GetUserAsync(ApiContext.GetUserId());
        await LogManager.LogRecoverAsync(rolesToReturn, channelsToReturn.ConvertAll(o => o.@override), guild, fromUser, user);

        if (rolesToReturn.Count > 0)
            await user.AddRolesAsync(rolesToReturn);

        foreach (var channel in channelsToReturn)
            await channel.channel.AddPermissionOverwriteAsync(user, channel.permissions);

        if (mutedRole != null && !data.KeepMutedRole)
            await user.RemoveRoleAsync(mutedRole);

        return ApiResult.Ok();
    }
}
