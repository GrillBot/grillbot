using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Suggestions;
using GrillBot.Database.Models;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class GetEmoteSuggestionsList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetEmoteSuggestionsList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public async Task<PaginatedResponse<EmoteSuggestion>> ProcessAsync(GetSuggestionsListParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.EmoteSuggestion.GetSuggestionListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<EmoteSuggestion>.CopyAndMapAsync(data, entity => Task.FromResult(Mapper.Map<EmoteSuggestion>(entity)));
    }
}
