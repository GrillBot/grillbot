using GrillBot.Common.Models;
using GrillBot.Common.Services.RubbergodService;
using GrillBot.Common.Services.RubbergodService.Models.Karma;

namespace GrillBot.App.Actions.Api.V2;

public class StoreKarma : ApiAction
{
    private IRubbergodServiceClient RubbergodServiceClient { get; }

    public StoreKarma(ApiRequestContext apiContext, IRubbergodServiceClient rubbergodServiceClient) : base(apiContext)
    {
        RubbergodServiceClient = rubbergodServiceClient;
    }

    public async Task ProcessAsync(List<KarmaItem> items)
    {
        await RubbergodServiceClient.StoreKarmaAsync(items);
    }
}
