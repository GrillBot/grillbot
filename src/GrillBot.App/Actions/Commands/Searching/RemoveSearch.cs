using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using SearchingService;

namespace GrillBot.App.Actions.Commands.Searching;

public class RemoveSearch(
    ITextsManager _texts,
    IServiceClientExecutor<ISearchingServiceClient> _searchingService
) : CommandAction
{
    public string? ErrorMessage { get; private set; }

    public async Task ProcessAsync(long id)
    {
        try
        {
            await _searchingService.ExecuteRequestAsync((c, ctx) => c.RemoveSearchingAsync(id, ctx.AuthorizationToken, ctx.CancellationToken));
        }
        catch (ClientBadRequestException ex) when (ex.ValidationErrors.Count > 0)
        {
            var validationError = ex.ValidationErrors.Values.SelectMany(o => o).First();
            ErrorMessage = _texts[validationError, Locale];
        }
        catch (ClientNotFoundException)
        {
            ErrorMessage = _texts["SearchingModule/RemoveSearch/NotFound", Locale];
        }
    }
}
