using Discord;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class UserBuilder : BuilderBase<IUser>
{
    public UserBuilder SetIdentity(ulong id, string username, string discriminator)
    {
        return SetId(id).SetUsername(username).SetDiscriminator(discriminator);
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

    public UserBuilder AsBot(bool isBot = true)
    {
        Mock.Setup(o => o.IsBot).Returns(isBot);

        if (isBot)
            Mock.Setup(o => o.IsWebhook).Returns(false);

        return this;
    }

    public UserBuilder AsWebhook(bool isWebhook = true)
    {
        Mock.Setup(o => o.IsWebhook).Returns(isWebhook);

        if (isWebhook)
            Mock.Setup(o => o.IsBot).Returns(false);

        return this;
    }

    public UserBuilder SetUsername(string username)
    {
        Mock.Setup(o => o.Username).Returns(username);
        return this;
    }

    public UserBuilder SetAvatarUrlAction(string avatarUrl, ImageFormat? format = null, ushort? size = null)
    {
        Mock.Setup(o => o.GetAvatarUrl(
            format != null ? It.Is<ImageFormat>(x => x == format.Value) : It.IsAny<ImageFormat>(),
            size != null ? It.Is<ushort>(x => x == size.Value) : It.IsAny<ushort>()
        )).Returns(avatarUrl);
        return this;
    }

    public UserBuilder SetStatus(UserStatus status)
    {
        Mock.Setup(o => o.Status).Returns(status);
        return this;
    }

    public UserBuilder SetAvatar(string avatarId)
    {
        Mock.Setup(o => o.AvatarId).Returns(avatarId);
        return this;
    }
}