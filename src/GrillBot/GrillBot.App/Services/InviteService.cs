using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Models.API.Invites;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Data.Models.Invite;
using GrillBot.Database.Enums;
using GrillBot.Database.Models;

namespace GrillBot.App.Services;

[Initializable]
public class InviteService
{
    private ConcurrentBag<InviteMetadata> MetadataCache { get; }
    private readonly object _metadataLock = new();
    private AuditLogService AuditLogService { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public InviteService(DiscordSocketClient discordClient, GrillBotDatabaseBuilder databaseBuilder,
        AuditLogService auditLogService, IMapper mapper)
    {
        MetadataCache = new ConcurrentBag<InviteMetadata>();
        AuditLogService = auditLogService;
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;

        DiscordClient.Ready += InitAsync;
        DiscordClient.UserJoined += user => user.IsUser() ? OnUserJoinedAsync(user) : Task.CompletedTask;
        DiscordClient.InviteCreated += OnInviteCreated;
    }

    private async Task InitAsync()
    {
        var invites = new List<InviteMetadata>();
        foreach (var guild in DiscordClient.Guilds)
        {
            var guildInvites = await GetLatestMetadataOfGuildAsync(guild);
            if (guildInvites == null) continue;

            var item = new AuditLogDataWrapper(AuditLogItemType.Info, $"Invites for guild {guild.Name} ({guild.Id}) loaded. Loaded invites: {guildInvites.Count}",
                guild, processedUser: DiscordClient.CurrentUser);
            await AuditLogService.StoreItemAsync(item);

            invites.AddRange(guildInvites);
        }

        lock (_metadataLock)
        {
            MetadataCache.Clear();
            invites.ForEach(invite => MetadataCache.Add(invite));
        }
    }

    private async Task<int> RefreshMetadataOfGuildAsync(SocketGuild guild)
    {
        var latestMetadata = await GetLatestMetadataOfGuildAsync(guild);
        if (latestMetadata == null) return 0;
        UpdateInvitesCache(latestMetadata, guild);

        return latestMetadata.Count(o => o.GuildId == guild.Id);
    }

    public async Task<Dictionary<string, int>> RefreshMetadataAsync()
    {
        var result = new Dictionary<string, int>();

        foreach (var guild in DiscordClient.Guilds)
        {
            var updatedCount = await RefreshMetadataOfGuildAsync(guild);
            result.Add(guild.Name, updatedCount);
        }

        return result;
    }

    public int GetMetadataCount()
    {
        lock (_metadataLock)
        {
            return MetadataCache.Count;
        }
    }

    private static async Task<List<InviteMetadata>> GetLatestMetadataOfGuildAsync(SocketGuild guild)
    {
        if (!guild.CurrentUser.GuildPermissions.CreateInstantInvite || !guild.CurrentUser.GuildPermissions.ManageGuild)
            return null;

        var invites = new List<InviteMetadata>();

        if (!string.IsNullOrEmpty(guild.VanityURLCode))
        {
            var vanityInvite = await guild.GetVanityInviteAsync();
            if (vanityInvite != null)
                invites.Add(InviteMetadata.FromDiscord(vanityInvite));
        }

        var guildInvites = (await guild.GetInvitesAsync()).Select(InviteMetadata.FromDiscord);
        invites.AddRange(guildInvites);

        return invites;
    }

    private async Task OnUserJoinedAsync(SocketGuildUser user)
    {
        var latestInvites = await GetLatestMetadataOfGuildAsync(user.Guild);
        var usedInvite = FindUsedInvite(user.Guild, latestInvites);
        await SetInviteToUserAsync(user, user.Guild, usedInvite, latestInvites);
    }

    private async Task SetInviteToUserAsync(IGuildUser user, IGuild guild, InviteMetadata usedInvite, List<InviteMetadata> latestInvites)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var joinedUserEntity = await repository.GuildUser.GetOrCreateGuildUserAsync(user);

        if (usedInvite == null)
        {
            var item = new AuditLogDataWrapper(AuditLogItemType.Warning, $"User {user.GetFullName()} ({user.Id}) used unknown invite.", guild, processedUser: user);
            await AuditLogService.StoreItemAsync(item);
        }
        else
        {
            if (usedInvite.CreatorId != null)
            {
                var creatorUser = await guild.GetUserAsync(usedInvite.CreatorId.Value);
                if (creatorUser != null)
                {
                    await repository.GuildUser.GetOrCreateGuildUserAsync(creatorUser);
                }
            }

            var invite = await repository.Invite.FindInviteByCodeAsync(guild, usedInvite.Code);
            if (invite == null)
            {
                invite = usedInvite.ToEntity();
                await repository.AddAsync(invite);
            }

            joinedUserEntity.UsedInviteCode = usedInvite.Code;
        }

        await repository.CommitAsync();
        UpdateInvitesCache(latestInvites, guild);
    }

    private void UpdateInvitesCache(List<InviteMetadata> invites, IGuild guild)
    {
        lock (_metadataLock)
        {
            invites.AddRange(MetadataCache.Where(o => o.GuildId != guild.Id));

            MetadataCache.Clear();
            invites.ForEach(invite => MetadataCache.Add(invite));
        }
    }

    private InviteMetadata FindUsedInvite(IGuild guild, List<InviteMetadata> latestData)
    {
        lock (_metadataLock)
        {
            var missingInvite = MetadataCache.FirstOrDefault(o => latestData.All(x => x.Code != o.Code));
            if (missingInvite != null) return missingInvite; // User joined via invite with max use limit.

            var result = MetadataCache
                .Where(o => o.GuildId == guild.Id)
                .FirstOrDefault(inv =>
                {
                    var fromLatest = latestData.Find(x => x.Code == inv.Code);
                    return fromLatest != null && fromLatest.Uses > inv.Uses;
                });

            return result ?? latestData.Find(inv => MetadataCache.All(o => o.Code != inv.Code));
        }
    }

    private Task OnInviteCreated(IInviteMetadata invite)
    {
        lock (_metadataLock)
        {
            var metadata = InviteMetadata.FromDiscord(invite);
            MetadataCache.Add(metadata);
        }

        return Task.CompletedTask;
    }

    public async Task<PaginatedResponse<GuildInvite>> GetInviteListAsync(GetInviteListParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Invite.GetInviteListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<GuildInvite>.CopyAndMapAsync(data, entity => Task.FromResult(Mapper.Map<GuildInvite>(entity)));
    }
}
