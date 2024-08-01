using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.SearchingService;

namespace GrillBot.App.Actions.Commands.Searching;

public class RemoveSearch : CommandAction
{
    private readonly ISearchingServiceClient _searchingService;
    private readonly ITextsManager _texts;

    public string? ErrorMessage { get; private set; }

    public RemoveSearch(ITextsManager texts, ISearchingServiceClient searchingService)
    {
        _searchingService = searchingService;
        _texts = texts;
    }

    public async Task ProcessAsync(long id)
    {
        try
        {
            await _searchingService.RemoveSearchingAsync(id);
        }
        catch (ClientBadRequestException ex) when (ex.ValidationErrors.Count > 0)
        {
            var validationError = ex.ValidationErrors.Values.SelectMany(o => o).First();
            ErrorMessage = _texts[validationError, Locale];
        }
    }
}
