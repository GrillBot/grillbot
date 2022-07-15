using AutoMapper;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Extensions;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.App.Services.Unverify;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Models;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Services.User;

public class UsersApiService
{
    private ApiRequestContext ApiRequestContext { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public UsersApiService(GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient dcClient,
        ApiRequestContext apiRequestContext, AuditLogWriter auditLogWriter)
    {
        ApiRequestContext = apiRequestContext;
        DatabaseBuilder = databaseBuilder;
        DiscordClient = dcClient;
        Mapper = mapper;
        AuditLogWriter = auditLogWriter;
    }

    public async Task<PaginatedResponse<UserListItem>> GetListAsync(GetUserListParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.User.GetUsersListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<UserListItem>.CopyAndMapAsync(data, MapItemAsync);
    }

    private async Task<UserListItem> MapItemAsync(Database.Entity.User entity)
    {
        var result = Mapper.Map<UserListItem>(entity);

        foreach (var guild in entity.Guilds.OrderBy(o => o.Guild!.Name))
        {
            var discordGuild = await DiscordClient.GetGuildAsync(guild.GuildId.ToUlong());
            var guildUser = discordGuild != null ? await discordGuild.GetUserAsync(guild.UserId.ToUlong()) : null;

            result.Guilds.Add(guild.Guild!.Name, guildUser != null);
        }

        return result;
    }

    public async Task<UserDetail> GetUserDetailAsync(ulong id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.User.FindUserWithDetailsByIdAsync(id);
        if (entity == null)
            return null;

        var result = Mapper.Map<UserDetail>(entity);
        var user = await DiscordClient.FindUserAsync(id);
        if (user != null)
            result = Mapper.Map(user, result);

        foreach (var guildUserEntity in entity.Guilds)
        {
            var guildUserDetail = Mapper.Map<GuildUserDetail>(guildUserEntity);

            guildUserDetail.CreatedInvites = guildUserDetail.CreatedInvites
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            guildUserDetail.Channels = guildUserDetail.Channels
                .OrderByDescending(o => o.Count)
                .ThenBy(o => o.Channel.Name)
                .ToList();

            guildUserDetail.Emotes = guildUserDetail.Emotes
                .OrderByDescending(o => o.UseCount)
                .ThenByDescending(o => o.LastOccurence)
                .ThenBy(o => o.Emote.Name)
                .ToList();

            var guild = await DiscordClient.GetGuildAsync(guildUserDetail.Guild.Id.ToUlong());
            if (guild != null)
            {
                guildUserDetail.IsGuildKnown = true;

                var guildUser = await guild.GetUserAsync(result.Id.ToUlong());
                guildUserDetail.IsUserInGuild = guildUserDetail.IsGuildKnown && guildUser != null;

                if (guildUser != null)
                {
                    if (guildUserEntity.Unverify != null)
                    {
                        var unverifyUserProfile = UnverifyProfileGenerator.Reconstruct(guildUserEntity.Unverify, guildUser, guild);
                        guildUserDetail.Unverify = Mapper.Map<UnverifyInfo>(unverifyUserProfile);
                    }

                    guildUserDetail.NicknameHistory = await GetUserNicknameHistoryAsync(repository, guildUser);
                }
            }

            result.Guilds.Add(guildUserDetail);
        }

        result.Guilds = result.Guilds
            .OrderByDescending(o => o.IsUserInGuild)
            .ThenBy(o => o.Guild.Name)
            .ToList();

        return result;
    }

    private static async Task<List<string>> GetUserNicknameHistoryAsync(GrillBotRepository repository, IGuildUser user)
    {
        var parameters = new AuditLogListParams
        {
            GuildId = user.GuildId.ToString(),
            Sort = { Descending = true },
            Types = new List<AuditLogItemType> { AuditLogItemType.MemberUpdated },
            IgnoreBots = true
        };

        var auditLogs = await repository.AuditLog.GetSimpleDataAsync(parameters);
        var result = new List<string>();

        foreach (var log in auditLogs)
        {
            var logData = JsonConvert.DeserializeObject<MemberUpdatedData>(log.Data, AuditLogWriter.SerializerSettings);
            if (logData == null || logData.Target.Id != user.Id || logData.Nickname == null) continue;

            if (!string.IsNullOrEmpty(logData.Nickname.Before)) result.Add(logData.Nickname.Before);
            if (!string.IsNullOrEmpty(logData.Nickname.After)) result.Add(logData.Nickname.After);
        }

        if (!string.IsNullOrEmpty(user.Nickname))
            result = result.FindAll(o => o != user.Nickname);

        return result.Distinct().ToList();
    }

    public async Task UpdateUserAsync(ulong id, UpdateUserParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var user = await repository.User.FindUserByIdAsync(id);

        if (user == null)
            throw new NotFoundException();

        var before = user.Clone();
        user.Note = parameters.Note;
        user.SelfUnverifyMinimalTime = parameters.SelfUnverifyMinimalTime;
        user.Flags = parameters.GetNewFlags(user.Flags);

        var auditLogItem = new AuditLogDataWrapper(
            AuditLogItemType.MemberUpdated,
            new MemberUpdatedData(before, user),
            processedUser: ApiRequestContext.LoggedUser
        );

        await AuditLogWriter.StoreAsync(auditLogItem);
        await repository.CommitAsync();
    }

    public async Task<List<UserPointsItem>> GetPointsBoardAsync()
    {
        var result = new List<UserPointsItem>();
        var mutualGuilds = (await DiscordClient.FindMutualGuildsAsync(ApiRequestContext.LoggedUser!.Id)).ConvertAll(o => o.Id.ToString());

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.GuildUser.GetPointsBoardDataAsync(mutualGuilds);
        if (data.Count > 0)
            result.AddRange(Mapper.Map<List<UserPointsItem>>(data));

        return result;
    }
}
