using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
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

    public async Task<EmoteStatItem> ProcessAsync(string emoteId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var emote = Discord.Emote.Parse(emoteId);
        var statistics = await repository.Emote.GetStatisticsOfEmoteAsync(emote);
        if (statistics is null)
            throw new NotFoundException();

        return Mapper.Map<EmoteStatItem>(statistics);
    }
}
