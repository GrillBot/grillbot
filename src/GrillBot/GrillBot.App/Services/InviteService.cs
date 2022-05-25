using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Invites;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Data.Models.Invite;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services;

[Initializable]
public class InviteService : ServiceBase
{
    private ConcurrentBag<InviteMetadata> MetadataCache { get; }
    private readonly object MetadataLock = new();
    private AuditLogService AuditLogService { get; }

    public InviteService(DiscordSocketClient discordClient, GrillBotContextFactory dbFactory,
        AuditLogService auditLogService, IMapper mapper) : base(discordClient, dbFactory, null, mapper)
    {
        MetadataCache = new ConcurrentBag<InviteMetadata>();
        AuditLogService = auditLogService;

        DiscordClient.Ready += () => InitAsync();
        DiscordClient.UserJoined += (user) => user.IsUser() ? OnUserJoinedAsync(user) : Task.CompletedTask;
        DiscordClient.InviteCreated += OnInviteCreated;
    }

    public async Task InitAsync()
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

        lock (MetadataLock)
        {
            MetadataCache.Clear();
            invites.ForEach(invite => MetadataCache.Add(invite));
        }
    }

    public async Task<int> RefreshMetadataOfGuildAsync(SocketGuild guild)
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
        lock (MetadataLock)
        {
            return MetadataCache.Count;
        }
    }

    static private async Task<List<InviteMetadata>> GetLatestMetadataOfGuildAsync(SocketGuild guild)
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

    public async Task OnUserJoinedAsync(SocketGuildUser user)
    {
        var latestInvites = await GetLatestMetadataOfGuildAsync(user.Guild);
        var usedInvite = FindUsedInvite(user.Guild, latestInvites);
        await SetInviteToUserAsync(user, user.Guild, usedInvite, latestInvites);
    }

    private async Task SetInviteToUserAsync(SocketGuildUser user, SocketGuild guild, InviteMetadata usedInvite, List<InviteMetadata> latestInvites)
    {
        var guildId = guild.Id.ToString();
        var userId = user.Id.ToString();

        using var dbContext = DbFactory.Create();

        await dbContext.InitGuildAsync(guild, CancellationToken.None);
        await dbContext.InitUserAsync(user, CancellationToken.None);

        var joinedUserEntity = await dbContext.GuildUsers.AsQueryable()
            .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == userId);

        if (joinedUserEntity == null)
        {
            joinedUserEntity = GuildUser.FromDiscord(guild, user);
            await dbContext.AddAsync(joinedUserEntity);
        }

        if (usedInvite == null)
        {
            var item = new AuditLogDataWrapper(AuditLogItemType.Warning, $"User {user.GetFullName()} ({user.Id}) used unknown invite.", guild, processedUser: user);
            await AuditLogService.StoreItemAsync(item);
        }
        else
        {
            if (usedInvite.CreatorId != null)
            {
                var creatorUser = guild.GetUser(usedInvite.CreatorId.Value);

                if (creatorUser != null)
                {
                    await dbContext.InitUserAsync(creatorUser, CancellationToken.None);
                    await dbContext.InitGuildUserAsync(guild, creatorUser, CancellationToken.None);
                }
            }

            var invite = await dbContext.Invites.AsQueryable()
                .FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.Code == usedInvite.Code);

            if (invite == null)
            {
                invite = usedInvite.ToEntity();
                await dbContext.AddAsync(invite);
            }

            joinedUserEntity.UsedInviteCode = usedInvite.Code;
        }

        await dbContext.SaveChangesAsync();
        UpdateInvitesCache(latestInvites, guild);
    }

    private void UpdateInvitesCache(List<InviteMetadata> invites, SocketGuild guild)
    {
        lock (MetadataLock)
        {
            invites.AddRange(MetadataCache.Where(o => o.GuildId != guild.Id));

            MetadataCache.Clear();
            invites.ForEach(invite => MetadataCache.Add(invite));
        }
    }

    private InviteMetadata FindUsedInvite(SocketGuild guild, List<InviteMetadata> latestData)
    {
        lock (MetadataLock)
        {
            var missingInvite = MetadataCache.FirstOrDefault(o => !latestData.Any(x => x.Code == o.Code));
            if (missingInvite != null) return missingInvite; // User joined via invite with max use limit.

            var result = MetadataCache
                .Where(o => o.GuildId == guild.Id)
                .FirstOrDefault(inv =>
                {
                    var fromLatest = latestData.Find(x => x.Code == inv.Code);
                    return fromLatest != null && fromLatest.Uses > inv.Uses;
                });

            return result ?? latestData.Find(inv => !MetadataCache.Any(o => o.Code == inv.Code));
        }
    }

    private Task OnInviteCreated(SocketInvite invite)
    {
        lock (MetadataLock)
        {
            var metadata = InviteMetadata.FromDiscord(invite);
            MetadataCache.Add(metadata);
        }

        return Task.CompletedTask;
    }

    public async Task<PaginatedResponse<GuildInvite>> GetInviteListAsync(GetInviteListParams parameters, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var query = context.CreateQuery(parameters, true);
        return await PaginatedResponse<GuildInvite>
            .CreateAsync(query, parameters.Pagination, entity => Mapper.Map<GuildInvite>(entity), cancellationToken);
    }
}
