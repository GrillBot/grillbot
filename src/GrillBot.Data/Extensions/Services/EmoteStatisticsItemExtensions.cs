using Emote.Models.Response;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.Data.Extensions.Services;

public static class EmoteStatisticsItemExtensions
{
    public static EmoteItem ToEmoteItem(this EmoteStatisticsItem item)
    {
        return new EmoteItem
        {
            FullId = $"<{(item.EmoteIsAnimated ? "a" : "")}:{item.EmoteName}:{item.EmoteId}>",
            Id = item.EmoteId,
            ImageUrl = item.EmoteUrl,
            Name = item.EmoteName
        };
    }
}
