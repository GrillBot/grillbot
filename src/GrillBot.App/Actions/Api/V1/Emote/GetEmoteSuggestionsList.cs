using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Suggestions;

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

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (GetSuggestionsListParams)Parameters[0]!;

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.EmoteSuggestion.GetSuggestionListAsync(parameters, parameters.Pagination);
        var result = await PaginatedResponse<EmoteSuggestion>.CopyAndMapAsync(data, entity => Task.FromResult(Mapper.Map<EmoteSuggestion>(entity)));

        return ApiResult.Ok(result);
    }
}
