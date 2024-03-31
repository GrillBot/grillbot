using GrillBot.Core.Services.Emote.Models.Response;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.Data.Extensions.Services;

public static class EmoteStatisticsItemExtensions
{
    public static string CreateFullEmoteId(this EmoteStatisticsItem item)
        => $"<{(item.EmoteIsAnimated ? "a" : "")}:{item.EmoteName}:{item.EmoteId}>";

    public static EmoteItem ToEmoteItem(this EmoteStatisticsItem item)
    {
        return new EmoteItem
        {
            FullId = item.CreateFullEmoteId(),
            Id = item.EmoteId,
            ImageUrl = item.EmoteUrl,
            Name = item.EmoteName
        };
    }
}
