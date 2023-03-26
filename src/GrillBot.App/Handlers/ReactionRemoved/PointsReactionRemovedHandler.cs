using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.PointsService.Models;

namespace GrillBot.App.Handlers.ReactionRemoved;

public class PointsReactionRemovedHandler : IReactionRemovedEvent
{
    private IPointsServiceClient PointsServiceClient { get; }

    private IGuildChannel? Channel { get; set; }
    private IEmote? Emote { get; set; }

    public PointsReactionRemovedHandler(IPointsServiceClient pointsServiceClient)
    {
        PointsServiceClient = pointsServiceClient;
    }

    public async Task ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        Init(cachedChannel, reaction);
        if (Channel is null || Emote is null) return;

        var reactionId = new ReactionInfo
        {
            Emote = Emote.ToString()!,
            UserId = reaction.UserId.ToString()
        }.GetReactionId();

        await PointsServiceClient.DeleteTransactionAsync(Channel!.GuildId.ToString(), cachedMessage.Id.ToString(), reactionId);
    }

    private void Init(Cacheable<IMessageChannel, ulong> cachedChannel, IReaction reaction)
    {
        Channel = cachedChannel is { HasValue: true, Value: IGuildChannel guildChannel } ? guildChannel : null;
        if (Channel is null) return;

        Emote = reaction.Emote is Emote ? Channel.Guild.Emotes.FirstOrDefault(x => x.IsEqual(reaction.Emote)) : null;
    }
}
