using Discord;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class UserBuilder : BuilderBase<IUser>
{
    public UserBuilder(ulong id, string username, string discriminator)
    {
        SetId(id);
        SetUsername(username);
        SetDiscriminator(discriminator);
    }

    public UserBuilder(IUser user) : this(user.Id, user.Username, user.Discriminator)
    {
    }

    public UserBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public UserBuilder SetDiscriminator(string discriminator)
    {
        var discriminatorValue = Convert.ToUInt16(discriminator);

        Mock.Setup(o => o.Discriminator).Returns(discriminator);
        Mock.Setup(o => o.DiscriminatorValue).Returns(discriminatorValue);
        Mock.Setup(o => o.GetDefaultAvatarUrl()).Returns(CDN.GetDefaultUserAvatarUrl(discriminatorValue));

        return this;
    }

    public UserBuilder SetUsername(string username)
    {
        Mock.Setup(o => o.Username).Returns(username);
        return this;
    }

    public UserBuilder SetAvatar(string avatarId)
    {
        Mock.Setup(o => o.AvatarId).Returns(avatarId);
        return this;
    }

    public UserBuilder SetSendMessageAction(IUserMessage message)
    {
        var dmChannel = new DmChannelBuilder().SetSendMessageAction(message).Build();
        Mock.Setup(o => o.CreateDMChannelAsync(It.IsAny<RequestOptions>())).ReturnsAsync(dmChannel);
        return this;
    }
}
