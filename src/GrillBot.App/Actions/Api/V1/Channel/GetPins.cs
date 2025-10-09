using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using RubbergodService;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GrillBot.Core.Services.Common.Executor;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class GetPins(
    ApiRequestContext apiContext,
    ChannelHelper _channelHelper,
    ITextsManager _texts,
    IServiceClientExecutor<IRubbergodServiceClient> _rubbergodServiceClient
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var channelId = GetParameter<ulong>(0);
        var markdown = GetParameter<bool>(1);

        var guild = await _channelHelper.GetGuildFromChannelAsync(null, channelId, CancellationToken)
            ?? throw new NotFoundException(_texts["ChannelModule/ChannelDetail/ChannelNotFound", ApiContext.Language]);

        using var response = await _rubbergodServiceClient.ExecuteRequestAsync(
            (c, ctx) => c.GetPinsAsync(guild.Id, channelId, markdown, ctx.CancellationToken),
            CancellationToken
        );

        var content = await response.ReadAsByteArrayAsync(CancellationToken);

        var apiResult = new ContentResult
        {
            Content = Encoding.UTF8.GetString(content),
            ContentType = markdown ? "text/markdown" : "application/json",
            StatusCode = StatusCodes.Status200OK
        };

        return ApiResult.Ok(apiResult);
    }
}
