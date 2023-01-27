using Discord;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class SelfUserBuilder : BuilderBase<ISelfUser>
{
    public SelfUserBuilder(ulong id, string username, string discriminator)
    {
        SetId(id);
        SetUsername(username);
        SetDiscriminator(discriminator);

        Mock.Setup(o => o.IsBot).Returns(true);
    }

    public SelfUserBuilder(IUser user) : this(user.Id, user.Username, user.Discriminator)
    {
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

    public SelfUserBuilder SetUsername(string username)
    {
        Mock.Setup(o => o.Username).Returns(username);
        return this;
    }
}
