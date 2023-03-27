using GrillBot.App.Helpers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.PointsService.Models;

namespace GrillBot.App.Handlers.ReactionAdded;

public class PointsReactionAddedHandler : IReactionAddedEvent
{
    private IMessageCacheManager MessageCache { get; }
    private PointsHelper PointsHelper { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    private IGuildChannel? Channel { get; set; }
    private IUser? ReactionUser { get; set; }
    private IUserMessage? Message { get; set; }

    public PointsReactionAddedHandler(IMessageCacheManager messageCache, PointsHelper pointsHelper, IPointsServiceClient pointsServiceClient)
    {
        MessageCache = messageCache;
        PointsHelper = pointsHelper;
        PointsServiceClient = pointsServiceClient;
    }

    public async Task ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        await InitAsync(cachedChannel, cachedMessage, reaction);

        if (Channel is null || ReactionUser is null || Message is null) return;
        if (!ReactionUser.IsUser()) return;
        if (!PointsHelper.CanIncrementPoints(Message) || Message.Author.Id == reaction.UserId) return;

        var request = new TransactionRequest
        {
            GuildId = Channel.GuildId.ToString(),
            ChannelId = Channel.Id.ToString(),
            MessageInfo = new MessageInfo
            {
                Id = Message.Id.ToString(),
                AuthorId = Message.Author.Id.ToString(),
                ContentLength = Message.Content.Length,
                MessageType = Message.Type
            },
            ReactionInfo = new ReactionInfo
            {
                UserId = reaction.UserId.ToString(),
                Emote = reaction.Emote.ToString()!
            }
        };

        var validationErrors = await PointsServiceClient.CreateTransactionAsync(request);
        if (PointsHelper.CanSyncData(validationErrors))
        {
            await PointsHelper.SyncDataWithServiceAsync(Channel.Guild, new[] { Message.Author, ReactionUser }, new[] { Channel });
            validationErrors = await PointsServiceClient.CreateTransactionAsync(request);
        }

        if (validationErrors is not null)
            throw new ValidationException(JsonConvert.SerializeObject(validationErrors));
    }

    private async Task InitAsync(Cacheable<IMessageChannel, ulong> cachedChannel, Cacheable<IUserMessage, ulong> cachedMessage, SocketReaction reaction)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not IGuildChannel guildChannel) return;
        if (reaction.Emote is not Emote || !guildChannel.Guild.Emotes.Any(x => x.IsEqual(reaction.Emote))) return;

        Channel = guildChannel;
        ReactionUser = reaction.User.IsSpecified ? reaction.User.Value : await Channel.Guild.GetUserAsync(reaction.UserId);

        if (cachedMessage.HasValue)
        {
            Message = cachedMessage.Value;
        }
        else
        {
            var message = await MessageCache.GetAsync(cachedMessage.Id, cachedChannel.Value);
            Message = message as IUserMessage;
        }
    }
}
