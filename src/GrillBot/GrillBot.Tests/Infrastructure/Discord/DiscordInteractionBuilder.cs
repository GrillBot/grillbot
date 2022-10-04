using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

public class DiscordInteractionBuilder : BuilderBase<IDiscordInteraction>
{
    public DiscordInteractionBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }
}
