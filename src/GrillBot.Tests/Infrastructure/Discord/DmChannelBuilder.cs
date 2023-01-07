using Discord;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Discord.Net;
using Moq;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class DmChannelBuilder : BuilderBase<IDMChannel>
{
    public DmChannelBuilder SetSendMessageAction(IUserMessage message, bool disabledDms = false)
    {
        var setup = Mock.Setup(o => o.SendMessageAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Embed>(), It.IsAny<RequestOptions>(), It.IsAny<AllowedMentions>(), It.IsAny<MessageReference>(),
            It.IsAny<MessageComponent>(), It.IsAny<ISticker[]>(), It.IsAny<Embed[]>(), It.IsAny<MessageFlags>()));

        if (disabledDms)
            setup.ThrowsAsync(new HttpException(HttpStatusCode.BadRequest, null, DiscordErrorCode.CannotSendMessageToUser));
        else
            setup.ReturnsAsync(message);

        return this;
    }
}
