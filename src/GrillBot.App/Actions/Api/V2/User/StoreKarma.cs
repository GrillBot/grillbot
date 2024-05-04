using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.App.Actions.Api.V2.User;

public class StoreKarma : ApiAction
{
    private readonly IRabbitMQPublisher _rabbitPublisher;

    public StoreKarma(ApiRequestContext apiContext, IRabbitMQPublisher rabbitPublisher) : base(apiContext)
    {
        _rabbitPublisher = rabbitPublisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var items = GetParameter<List<RawKarmaItem>>(0);
        var payloads = items.ConvertAll(o => o.ToPayload());

        await _rabbitPublisher.PublishBatchAsync(payloads, new());
        return ApiResult.Ok();
    }
}
