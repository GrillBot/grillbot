﻿using GrillBot.App.Managers;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.UserJoined;

public class InviteUserJoinedHandler : IUserJoinedEvent
{
    private IDiscordClient DiscordClient { get; }
    private InviteManager InviteManager { get; }
    private AuditLogWriteManager AuditLogWriteManagerWriteManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public InviteUserJoinedHandler(IDiscordClient discordClient, InviteManager inviteManager, AuditLogWriteManager auditLogWriteManager, GrillBotDatabaseBuilder databaseBuilder)
    {
        DiscordClient = discordClient;
        InviteManager = inviteManager;
        AuditLogWriteManagerWriteManager = auditLogWriteManager;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IGuildUser user)
    {
        if (!user.IsUser() || !await user.Guild.CanManageInvitesAsync(DiscordClient.CurrentUser)) return;

        var latestInvites = await InviteManager.DownloadInvitesAsync(user.Guild);
        var usedInvite = await FindUsedInviteAsync(user.Guild, latestInvites);
        await SetInviteToUserAsync(user, user.Guild, usedInvite, latestInvites);
    }

    private async Task<Cache.Entity.InviteMetadata?> FindUsedInviteAsync(IGuild guild, List<IInviteMetadata> latestData)
    {
        var cachedInvites = await InviteManager.GetInvitesAsync(guild);

        // Try find invite with limited count of usage. If invite was used as last, discord will automatically remove this invite.
        var missingInvite = cachedInvites.FirstOrDefault(o => !latestData.Select(x => x.Code).Contains(o.Code));
        if (missingInvite != null) return missingInvite;

        // Find invite which have incremented use count against the cache.
        var result = cachedInvites
            .Select(o => new { Cached = o, Current = latestData.Find(x => x.Code == o.Code) })
            .FirstOrDefault(o => o.Current != null && o.Current.Uses > o.Cached.Uses);

        return InviteManager.ConvertMetadata(result?.Current);
    }

    private async Task SetInviteToUserAsync(IGuildUser user, IGuild guild, Cache.Entity.InviteMetadata? usedInvite, IEnumerable<IInviteMetadata> latestInvites)
    {
        if (usedInvite == null)
        {
            var item = new AuditLogDataWrapper(AuditLogItemType.Warning, $"User {user.GetFullName()} ({user.Id}) used unknown invite.", guild, processedUser: user);
            await AuditLogWriteManagerWriteManager.StoreAsync(item);
            return;
        }

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(guild);
        await repository.User.GetOrCreateUserAsync(user);

        if (usedInvite.CreatorId != null)
        {
            var creatorUser = await guild.GetUserAsync(usedInvite.CreatorId.ToUlong());
            if (creatorUser != null && creatorUser.Id != user.Id)
            {
                await repository.User.GetOrCreateUserAsync(creatorUser);
                await repository.GuildUser.GetOrCreateGuildUserAsync(creatorUser);
            }
        }

        var invite = await repository.Invite.FindInviteByCodeAsync(guild.Id, usedInvite.Code);
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
}
