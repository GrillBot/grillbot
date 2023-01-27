using Discord;
using GrillBot.Cache.Entity;

namespace GrillBot.Tests.Infrastructure.Discord;

public class InviteMetadataBuilder : BuilderBase<IInviteMetadata>
{
    public InviteMetadataBuilder SetCode(string code)
    {
        Mock.Setup(o => o.Code).Returns(code);
        Mock.Setup(o => o.Url).Returns(DiscordConfig.InviteUrl + code);
        return this;
    }

    public InviteMetadataBuilder SetGuild(IGuild guild)
    {
        Mock.Setup(o => o.Guild).Returns(guild);
        Mock.Setup(o => o.GuildId).Returns(guild.Id);
        Mock.Setup(o => o.GuildName).Returns(guild.Name);
        return this;
    }

    public InviteMetadataBuilder SetUses(int uses)
    {
        Mock.Setup(o => o.Uses).Returns(uses);
        return this;
    }
}
