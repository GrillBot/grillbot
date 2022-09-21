using Discord;
using Discord.WebSocket;
using GrillBot.Cache.Services.Managers.MessageCache;
using Moq;

namespace GrillBot.Tests.Infrastructure.Cache;

public class MessageCacheBuilder : BuilderBase<IMessageCacheManager>
{
    public MessageCacheBuilder SetGetAction(ulong messageId, IMessage returns)
    {
        Mock.Setup(o => o.GetAsync(It.Is<ulong>(x => x == messageId), It.IsAny<IMessageChannel>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(returns);
        return this;
    }
}
