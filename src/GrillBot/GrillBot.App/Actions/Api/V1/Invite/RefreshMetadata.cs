using GrillBot.App.Managers;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Invite;

public class RefreshMetadata : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private InviteManager InviteManager { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }

    public RefreshMetadata(ApiRequestContext apiContext, IDiscordClient discordClient, InviteManager inviteManager, AuditLogWriteManager auditLogWriteManager) : base(apiContext)
    {
        DiscordClient = discordClient;
        InviteManager = inviteManager;
        AuditLogWriteManager = auditLogWriteManager;
    }

    public async Task<Dictionary<string, int>> ProcessAsync(bool isReload)
    {
        var result = new Dictionary<string, int>();
        var guilds = await DiscordClient.GetGuildsAsync();

        foreach (var guild in guilds)
        {
            var count = await RefreshMetadataAsync(guild, isReload);
            result.Add(guild.Name, count);
        }

        return result;
    }

    private async Task<int> RefreshMetadataAsync(IGuild guild, bool isReload)
    {
        if (!await guild.CanManageInvitesAsync(DiscordClient.CurrentUser)) return 0;

        var invites = await InviteManager.DownloadInvitesAsync(guild);
        if (invites.Count == 0) return 0;

        await AuditLogWriteManager.StoreAsync(new AuditLogDataWrapper(AuditLogItemType.Info,
            $"Invites for guild \"{guild.Name}\" was {(isReload ? "reloaded" : "loaded")}. Loaded invites: {invites.Count}", guild, processedUser: ApiContext.LoggedUser));

        await InviteManager.UpdateMetadataAsync(guild, invites);
        return invites.Count;
    }
}
