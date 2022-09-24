using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services;

[Initializable]
public class InviteService
{
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private AuditLogWriter AuditLogWriter { get; }
    private InviteManager InviteManager { get; }
    private IServiceProvider ServiceProvider { get; }

    public InviteService(DiscordSocketClient discordClient, GrillBotDatabaseBuilder databaseBuilder, AuditLogWriter auditLogWriter, InviteManager inviteManager, IServiceProvider serviceProvider)
    {
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
        AuditLogWriter = auditLogWriter;
        InviteManager = inviteManager;
        ServiceProvider = serviceProvider;

        DiscordClient.Ready += InitAsync;
        DiscordClient.UserJoined += user => user.IsUser() ? OnUserJoinedAsync(user) : Task.CompletedTask;
        DiscordClient.InviteCreated += OnInviteCreated;
    }

    private async Task InitAsync()
    {
        using var scope = ServiceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApiRequestContext>().LoggedUser = DiscordClient.CurrentUser;

        var action = scope.ServiceProvider.GetRequiredService<Actions.Api.V1.Invite.RefreshMetadata>();
        await action.ProcessAsync(false);
    }

    private async Task OnUserJoinedAsync(SocketGuildUser user)
    {
        if (!await user.Guild.CanManageInvitesAsync(DiscordClient.CurrentUser)) return;

        var latestInvites = await InviteManager.DownloadInvitesAsync(user.Guild);
        var usedInvite = await FindUsedInviteAsync(user.Guild, latestInvites);
        await SetInviteToUserAsync(user, user.Guild, usedInvite, latestInvites);
    }

    private async Task SetInviteToUserAsync(IGuildUser user, IGuild guild, Cache.Entity.InviteMetadata usedInvite, IEnumerable<IInviteMetadata> latestInvites)
    {
        if (usedInvite == null)
        {
            var item = new AuditLogDataWrapper(AuditLogItemType.Warning, $"User {user.GetFullName()} ({user.Id}) used unknown invite.", guild, processedUser: user);
            await AuditLogWriter.StoreAsync(item);
            return;
        }

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateRepositoryAsync(guild);
        await repository.User.GetOrCreateUserAsync(user);

        if (usedInvite.CreatorId != null)
        {
            var creatorUser = await guild.GetUserAsync(usedInvite.CreatorId.ToUlong());
            if (creatorUser != null)
            {
                await repository.User.GetOrCreateUserAsync(creatorUser);
                await repository.GuildUser.GetOrCreateGuildUserAsync(creatorUser);
            }
        }

        var invite = await repository.Invite.FindInviteByCodeAsync(guild, usedInvite.Code);
        if (invite == null)
        {
            invite = new Database.Entity.Invite
            {
                Code = usedInvite.Code,
                CreatedAt = usedInvite.CreatedAt,
                CreatorId = usedInvite.CreatorId,
                GuildId = usedInvite.GuildId
            };
            await repository.AddAsync(invite);
        }

        var joinedUserEntity = await repository.GuildUser.GetOrCreateGuildUserAsync(user);
        joinedUserEntity.UsedInviteCode = usedInvite.Code;

        await repository.CommitAsync();
        await InviteManager.UpdateMetadataAsync(guild, latestInvites);
    }

    private async Task<Cache.Entity.InviteMetadata> FindUsedInviteAsync(IGuild guild, List<IInviteMetadata> latestData)
    {
        var cachedInvites = await InviteManager.GetInvitesAsync(guild);

        var missingInvite = cachedInvites.FirstOrDefault(o => latestData.All(x => x.Code != o.Code));
        if (missingInvite != null) return missingInvite; // User joined via invite with max use limit.

        // Find used invite which have incremented use value against the cache.
        var result = cachedInvites.FirstOrDefault(o =>
        {
            var fromLatest = latestData.Find(x => x.Code == o.Code);
            return fromLatest != null && fromLatest.Uses > o.Uses;
        });

        if (result != null)
            return result;

        var lastChance = latestData.Find(o => cachedInvites.All(x => x.Code != o.Code));
        return lastChance == null ? null : InviteManager.ConvertMetadata(lastChance);
    }

    private async Task OnInviteCreated(IInviteMetadata invite)
        => await InviteManager.AddInviteAsync(invite);
}
