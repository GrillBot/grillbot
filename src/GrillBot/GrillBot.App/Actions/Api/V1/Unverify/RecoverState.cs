using GrillBot.App.Services.Unverify;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class RecoverState : ApiAction
{
    private UnverifyService UnverifyService { get; }

    public RecoverState(ApiRequestContext apiContext, UnverifyService unverifyService) : base(apiContext)
    {
        UnverifyService = unverifyService;
    }

    public async Task ProcessAsync(long logId)
        => await UnverifyService.RecoverUnverifyState(logId, ApiContext.GetUserId(), ApiContext.Language);
}
