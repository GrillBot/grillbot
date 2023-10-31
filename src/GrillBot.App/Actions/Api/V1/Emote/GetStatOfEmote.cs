using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class GetStatOfEmote : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetStatOfEmote(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var emoteId = (string)Parameters[0]!;

        await using var repository = DatabaseBuilder.CreateRepository();

        var emote = Discord.Emote.Parse(emoteId);
        var statistics = await repository.Emote.GetStatisticsOfEmoteAsync(emote)
            ?? throw new NotFoundException();

        var result = Mapper.Map<EmoteStatItem>(statistics);
        return ApiResult.Ok(result);
    }
}
