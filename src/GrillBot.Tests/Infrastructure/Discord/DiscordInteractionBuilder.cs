using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

public class DiscordInteractionBuilder : BuilderBase<IDiscordInteraction>
{
    public DiscordInteractionBuilder(ulong id)
    {
        SetId(id);
    }

    public DiscordInteractionBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public DiscordInteractionBuilder AsDmInteraction(bool isDmInteraction = true)
    {
        Mock.Setup(o => o.IsDMInteraction).Returns(isDmInteraction);
        return this;
    }
}
