using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Extensions;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Users;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using System.Security.Claims;

namespace GrillBot.App.Services.User;

public class UsersApiService : ServiceBase
{
    private AuditLogService AuditLogService { get; }

    public UsersApiService(GrillBotContextFactory dbFactory, IMapper mapper, IDiscordClient dcClient,
        AuditLogService auditLogService) : base(null, dbFactory, null, dcClient, mapper)
    {
        AuditLogService = auditLogService;
    }

    public async Task<PaginatedResponse<UserListItem>> GetListAsync(GetUserListParams parameters, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var query = context.CreateQuery(parameters, true);

        return await PaginatedResponse<UserListItem>
            .CreateAsync(query, parameters.Pagination, (entity, cancellationToken) => MapItemAsync(entity, cancellationToken), cancellationToken);
    }

    private async Task<UserListItem> MapItemAsync(Database.Entity.User entity, CancellationToken cancellationToken = default)
    {
        var result = Mapper.Map<UserListItem>(entity);
        var discordUser = await DcClient.FindUserAsync(entity.Id.ToUlong(), cancellationToken);

        if (discordUser != null)
            result = Mapper.Map(discordUser, result);

        foreach (var guild in entity.Guilds)
        {
            var discordGuild = await DcClient.GetGuildAsync(guild.GuildId.ToUlong(), options: new() { CancelToken = cancellationToken });
            var guildUser = discordGuild != null ? await discordGuild.GetUserAsync(guild.UserId.ToUlong(), options: new() { CancelToken = cancellationToken }) : null;

            result.Guilds.Add(guild.Guild.Name, guildUser != null);
        }

        return result;
    }

    public async Task<UserDetail> GetUserDetailAsync(ulong id, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var query = context.Users
            .Include(o => o.Guilds).ThenInclude(o => o.Guild)
            .Include(o => o.Guilds).ThenInclude(o => o.UsedInvite.Creator.User)
            .Include(o => o.Guilds).ThenInclude(o => o.CreatedInvites.Where(x => x.UsedUsers.Count > 0))
            .Include(o => o.Guilds).ThenInclude(o => o.Channels.Where(x => x.Count > 0)).ThenInclude(o => o.Channel)
            .Include(o => o.Guilds).ThenInclude(o => o.EmoteStatistics.Where(x => x.UseCount > 0))
            .Where(o => o.Id == id.ToString())
            .AsQueryable();
        query = query.AsNoTracking().AsSplitQuery();

        var entity = await query.FirstOrDefaultAsync(cancellationToken);
        if (entity == null)
            return null;

        var result = Mapper.Map<UserDetail>(entity);
        var user = await DcClient.FindUserAsync(id, cancellationToken);
        if (user != null)
            result = Mapper.Map(user, result);

        foreach (var guildUserEntity in entity.Guilds)
        {
            var guildUser = Mapper.Map<GuildUserDetail>(guildUserEntity);

            guildUser.CreatedInvites = guildUser.CreatedInvites
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            guildUser.Channels = guildUser.Channels
                .OrderByDescending(o => o.Count)
                .ThenBy(o => o.Channel.Name)
                .ToList();

            guildUser.Emotes = guildUser.Emotes
                .OrderByDescending(o => o.UseCount)
                .ThenByDescending(o => o.LastOccurence)
                .ThenBy(o => o.Emote.Name)
                .ToList();

            var guild = await DcClient.GetGuildAsync(guildUser.Guild.Id.ToUlong(), options: new() { CancelToken = cancellationToken });

            guildUser.IsGuildKnown = guild != null;
            guildUser.IsUserInGuild = guildUser.IsGuildKnown && (await guild.GetUserAsync(result.Id.ToUlong())) != null;
            result.Guilds.Add(guildUser);
        }

        result.Guilds = result.Guilds
            .OrderByDescending(o => o.IsUserInGuild)
            .ThenBy(o => o.Guild.Name)
            .ToList();

        return result;
    }

    public async Task UpdateUserAsync(ulong id, UpdateUserParams parameters, ClaimsPrincipal loggedUser)
    {
        using var context = DbFactory.Create();

        var user = await context.Users.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == id.ToString());

        if (user == null)
            throw new NotFoundException();

        var before = user.Clone();
        user.Note = parameters.Note;
        user.SelfUnverifyMinimalTime = parameters.SelfUnverifyMinimalTime;
        user.Flags = parameters.GetNewFlags(user.Flags);

        var auditLogItem = new AuditLogDataWrapper(
            AuditLogItemType.MemberUpdated,
            new MemberUpdatedData(before, user),
            processedUser: await DcClient.FindUserAsync(loggedUser.GetUserId())
        );

        await AuditLogService.StoreItemAsync(auditLogItem);
        await context.SaveChangesAsync();
    }

    public async Task SetHearthbeatStatusAsync(ClaimsPrincipal loggedUser, bool online)
    {
        var userId = loggedUser.GetUserId().ToString();
        var isPublic = loggedUser.HaveUserPermission();

        using var context = DbFactory.Create();

        var user = await context.Users.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == userId);

        if (online)
            user.Flags |= (int)(isPublic ? UserFlags.PublicAdminOnline : UserFlags.WebAdminOnline);
        else
            user.Flags &= ~(int)(isPublic ? UserFlags.PublicAdminOnline : UserFlags.WebAdminOnline);

        await context.SaveChangesAsync();
    }

    public async Task<List<UserPointsItem>> GetPointsBoardAsync(ClaimsPrincipal loggedUser, CancellationToken cancellationToken = default)
    {
        var loggedUserId = loggedUser.GetUserId();
        var result = new List<UserPointsItem>();
        var mutualGuilds = (await DcClient.FindMutualGuildsAsync(loggedUserId)).ConvertAll(o => o.Id.ToString());

        using var context = DbFactory.Create();

        var query = context.GuildUsers
            .Include(o => o.Guild)
            .Include(o => o.User)
            .Where(o => o.Points > 0 && (o.User.Flags & (int)UserFlags.NotUser) == 0 && mutualGuilds.Contains(o.GuildId) && !o.User.Username.StartsWith("Imported"))
            .OrderByDescending(o => o.Points)
            .ThenBy(o => o.Nickname).ThenBy(o => o.User.Username).ThenBy(o => o.User.Discriminator)
            .AsQueryable();

        query = query.AsNoTracking();

        var data = await query.ToListAsync(cancellationToken);
        if (data.Count > 0)
            result.AddRange(Mapper.Map<List<UserPointsItem>>(data));

        return result;
    }
}
