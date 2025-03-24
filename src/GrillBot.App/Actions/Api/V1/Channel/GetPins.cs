using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Services.RubbergodService;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GrillBot.Core.Services.Common.Executor;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class GetPins : ApiAction
{
    private ChannelHelper ChannelHelper { get; }
    private ITextsManager Texts { get; }
    private readonly IServiceClientExecutor<IRubbergodServiceClient> _rubbergodServiceClient;

    public GetPins(ApiRequestContext apiContext, ChannelHelper channelHelper, ITextsManager texts, IServiceClientExecutor<IRubbergodServiceClient> rubbergodServiceClient) : base(apiContext)
    {
        ChannelHelper = channelHelper;
        Texts = texts;
        _rubbergodServiceClient = rubbergodServiceClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var channelId = (ulong)Parameters[0]!;
        var markdown = (bool)Parameters[1]!;

        var guild = await ChannelHelper.GetGuildFromChannelAsync(null, channelId)
            ?? throw new NotFoundException(Texts["ChannelModule/ChannelDetail/ChannelNotFound", ApiContext.Language]);

        var content = await _rubbergodServiceClient.ExecuteRequestAsync((c, cancellationToken) => c.GetPinsAsync(guild.Id, channelId, markdown, cancellationToken));
        var apiResult = new ContentResult
        {
            Content = Encoding.UTF8.GetString(content),
            ContentType = markdown ? "text/markdown" : "application/json",
            StatusCode = StatusCodes.Status200OK
        };

        return ApiResult.Ok(apiResult);
    }
}
