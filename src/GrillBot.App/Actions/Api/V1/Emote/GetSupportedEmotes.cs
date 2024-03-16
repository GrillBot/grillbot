using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Emote;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class GetSupportedEmotes : ApiAction
{
    private readonly IEmoteServiceClient _emoteServiceClient;
    private readonly DataResolveManager _dataResolve;

    public GetSupportedEmotes(ApiRequestContext apiContext, IEmoteServiceClient emoteServiceClient, DataResolveManager dataResolve) : base(apiContext)
    {
        _emoteServiceClient = emoteServiceClient;
        _dataResolve = dataResolve;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var supportedEmotes = await _emoteServiceClient.GetSupportedEmotesListAsync();
        var result = new List<GuildEmoteItem>();

        foreach (var item in supportedEmotes)
        {
            var guild = await _dataResolve.GetGuildAsync(item.GuildId.ToUlong());
            var emoteItem = Discord.Emote.Parse(item.FullId);

            result.Add(new GuildEmoteItem
            {
                FullId = item.FullId,
                Guild = guild!,
                Id = emoteItem.Id.ToString(),
                ImageUrl = emoteItem.Url,
                Name = emoteItem.Name
            });
        }

        result = result.OrderBy(o => o.Name).ToList();
        return ApiResult.Ok(result);
    }
}
