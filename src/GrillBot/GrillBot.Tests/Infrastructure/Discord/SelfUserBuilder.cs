using Discord;
using Moq;
using System;

namespace GrillBot.Tests.Infrastructure.Discord;

public class SelfUserBuilder : BuilderBase<ISelfUser>
{
    public SelfUserBuilder SetPremiumType(PremiumType type)
    {
        Mock.Setup(o => o.PremiumType).Returns(type);
        return this;
    }

    public SelfUserBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public SelfUserBuilder SetDiscriminator(string discriminator)
    {
        var discriminatorValue = Convert.ToUInt16(discriminator);

        Mock.Setup(o => o.Discriminator).Returns(discriminator);
        Mock.Setup(o => o.DiscriminatorValue).Returns(discriminatorValue);
        Mock.Setup(o => o.GetDefaultAvatarUrl()).Returns(CDN.GetDefaultUserAvatarUrl(discriminatorValue));

        return this;
    }

    public SelfUserBuilder AsBot(bool isBot = true)
    {
        Mock.Setup(o => o.IsBot).Returns(isBot);
        return this;
    }

    public SelfUserBuilder AsWebhook(bool isWebhook = true)
    {
        Mock.Setup(o => o.IsWebhook).Returns(isWebhook);
        return this;
    }

    public SelfUserBuilder SetUsername(string username)
    {
        Mock.Setup(o => o.Username).Returns(username);
        return this;
    }

    public SelfUserBuilder SetAvatarUrlAction(string avatarUrl, ImageFormat? format = null, ushort? size = null)
    {
        Mock.Setup(o => o.GetAvatarUrl(
            format != null ? It.Is<ImageFormat>(x => x == format.Value) : It.IsAny<ImageFormat>(),
            size != null ? It.Is<ushort>(x => x == size.Value) : It.IsAny<ushort>()
        )).Returns(avatarUrl);
        return this;
    }
}
