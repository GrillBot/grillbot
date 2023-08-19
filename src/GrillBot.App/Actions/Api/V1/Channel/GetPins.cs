using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Services.RubbergodService;
using GrillBot.Core.Exceptions;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class GetPins : ApiAction
{
    private ChannelHelper ChannelHelper { get; }
    private ITextsManager Texts { get; }
    private IRubbergodServiceClient RubbergodServiceClient { get; }

    public GetPins(ApiRequestContext apiContext, ChannelHelper channelHelper, ITextsManager texts, IRubbergodServiceClient rubbergodServiceClient) : base(apiContext)
    {
        ChannelHelper = channelHelper;
        Texts = texts;
        RubbergodServiceClient = rubbergodServiceClient;
    }

    public async Task<string> ProcessAsync(ulong channelId, bool markdown)
    {
        var guild = await ChannelHelper.GetGuildFromChannelAsync(null, channelId);
        if (guild is null)
            throw new NotFoundException(Texts["ChannelModule/ChannelDetail/ChannelNotFound", ApiContext.Language]);

        var content = await RubbergodServiceClient.GetPinsAsync(guild.Id, channelId, markdown);
        return Encoding.UTF8.GetString(content);
    }
}
