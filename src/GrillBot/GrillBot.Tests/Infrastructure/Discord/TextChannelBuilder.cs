﻿using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

public class TextChannelBuilder : BuilderBase<ITextChannel>
{
    public TextChannelBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public TextChannelBuilder SetName(string name)
    {
        Mock.Setup(o => o.Name).Returns(name);
        return this;
    }

    public TextChannelBuilder SetNsfw(bool isNsfw)
    {
        Mock.Setup(o => o.IsNsfw).Returns(isNsfw);
        return this;
    }

    public TextChannelBuilder SetGuild(IGuild guild)
    {
        Mock.Setup(o => o.Guild).Returns(guild);
        Mock.Setup(o => o.GuildId).Returns(guild.Id);
        return this;
    }
}
