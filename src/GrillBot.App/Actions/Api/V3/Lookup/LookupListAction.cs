using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.API.Guilds;
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
            _ => Task.FromException<ApiResult>(new NotSupportedException())
        };
    }

    private async Task<ApiResult> ResolveGuildListAsync()
    {
        if (ApiContext.IsPublic())
        {
            var loggedUserId = ApiContext.GetUserId();
            var mutualGuilds = await _discordClient.FindMutualGuildsAsync(loggedUserId);

            return ApiResult.Ok(_mapper.Map<Guild>(mutualGuilds));
        }

        var query = _dbContext.Guilds.AsNoTracking().OrderBy(o => o.Name);
        var mappedQuery = _mapper.ProjectTo<Guild>(query);
        var guilds = await mappedQuery.ToListAsync();

        return ApiResult.Ok(guilds);
    }
}
