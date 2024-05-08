using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.RubbergodService.Models.Events.Karma;
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

        var batches = items
            .Select(o => new KarmaUser(o.MemberId, o.KarmaValue, o.Positive, o.Negative))
            .Chunk(100)
            .Select(ch => new KarmaBatchPayload(ch))
            .ToList();

        await _rabbitPublisher.PublishBatchAsync(batches, new());
        return ApiResult.Ok();
    }
}
