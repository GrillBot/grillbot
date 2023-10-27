using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.App.Actions.Api.V1.Guild;

public class GetGuildList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IDiscordClient DiscordClient { get; }

    public GetGuildList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (GetGuildListParams)Parameters[0]!;

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Guild.GetGuildListAsync(parameters, parameters.Pagination);
        var result = await PaginatedResponse<Data.Models.API.Guilds.Guild>
            .CopyAndMapAsync(data, entity => Task.FromResult(Mapper.Map<Data.Models.API.Guilds.Guild>(entity)));

        for (var i = 0; i < result.Data.Count; i++)
        {
            var guild = await DiscordClient.GetGuildAsync(result.Data[i].Id.ToUlong());
            if (guild == null) continue;

            result.Data[i] = Mapper.Map(guild, result.Data[i]);
        }

        return ApiResult.Ok(result);
    }
}
