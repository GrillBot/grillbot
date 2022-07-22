using AutoMapper;
using GrillBot.Data.Models.API.Suggestions;
using GrillBot.Database.Models;

namespace GrillBot.App.Services.Suggestion;

public class EmoteSuggestionApiService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public EmoteSuggestionApiService(GrillBotDatabaseBuilder databaseBuilder, IMapper mapper)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public async Task<PaginatedResponse<EmoteSuggestion>> GetListAsync(GetSuggestionsListParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.EmoteSuggestion.GetSuggestionListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<EmoteSuggestion>
            .CopyAndMapAsync(data, entity => Task.FromResult(Mapper.Map<EmoteSuggestion>(entity)));
    }
}
