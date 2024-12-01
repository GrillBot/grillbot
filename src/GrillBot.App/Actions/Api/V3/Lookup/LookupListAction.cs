using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.App.Actions.Api.V3.Lookup;

public class LookupListAction : ApiAction
{
    private readonly IDiscordClient _discordClient;
    private readonly IMapper _mapper;
    private readonly GrillBotContext _dbContext;

    public LookupListAction(ApiRequestContext apiContext, IDiscordClient discordClient, IMapper mapper, GrillBotContext dbContext) : base(apiContext)
    {
        _discordClient = discordClient;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public override Task<ApiResult> ProcessAsync()
    {
        var type = GetParameter<DataResolveType>(0);

        return type switch
        {
            DataResolveType.Guild => ResolveGuildListAsync(),
            DataResolveType.User => ResolveUserListAsync(),
            DataResolveType.Channel => ResolveChannelListAsync(),
            _ => Task.FromException<ApiResult>(new NotSupportedException())
        };
    }

    private async Task<ApiResult> ResolveGuildListAsync()
    {
        if (ApiContext.IsPublic())
        {
            var mutualGuilds = await GetMutualGuildsAsync();
            return ApiResult.Ok(_mapper.Map<Guild>(mutualGuilds));
        }

        var query = _dbContext.Guilds.AsNoTracking().OrderBy(o => o.Name);
        var mappedQuery = _mapper.ProjectTo<Guild>(query);
        var guilds = await mappedQuery.ToListAsync();

        return ApiResult.Ok(guilds);
    }

    private async Task<ApiResult> ResolveUserListAsync()
    {
        var query = _dbContext.Users
            .OrderBy(o => o.GlobalAlias ?? o.Username)
            .ThenBy(o => o.Username)
            .ThenBy(o => o.Id)
            .AsNoTracking();

        if (ApiContext.IsPublic())
        {
            var mutualGuilds = (await GetMutualGuildsAsync()).Select(o => o.Id.ToString()).ToArray();
            query = query.Where(o => o.Guilds.Any(g => mutualGuilds.Contains(g.GuildId)));
        }

        var mappedQuery = _mapper.ProjectTo<User>(query);
        var users = await mappedQuery.ToListAsync();

        return ApiResult.Ok(users);
    }

    private Task<List<IGuild>> GetMutualGuildsAsync()
    {
        var loggedUserId = ApiContext.GetUserId();
        return _discordClient.FindMutualGuildsAsync(loggedUserId);
    }

    private async Task<ApiResult> ResolveChannelListAsync()
    {
        if (ApiContext.IsPublic())
        {
            var guilds = await GetMutualGuildsAsync();
            var visibleChannels = new List<Channel>();
            var userId = ApiContext.GetUserId();

            foreach (var guild in guilds)
            {
                var user = await guild.GetUserAsync(userId);
                visibleChannels.AddRange(
                    (await guild.GetAvailableChannelsAsync(user))
                        .Select(_mapper.Map<Channel>)
                        .Where(ch => ch.Type != ChannelType.Category && ch.Type != ChannelType.DM)
                );
            }

            return ApiResult.Ok(visibleChannels);
        }

        var baseQuery = _dbContext.Channels
            .Where(ch => ch.ChannelType != ChannelType.Category && ch.ChannelType != ChannelType.DM)
            .OrderBy(o => o.Name)
            .AsNoTracking();

        var query = _mapper.ProjectTo<Channel>(baseQuery);
        var channels = await query.ToListAsync();

        return ApiResult.Ok(channels);
    }
}
