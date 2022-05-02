using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

public class RoleBuilder : BuilderBase<IRole>
{
    public RoleBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public RoleBuilder SetColor(Color color)
    {
        Mock.Setup(o => o.Color).Returns(color);
        return this;
    }

    public RoleBuilder SetName(string name)
    {
        Mock.Setup(o => o.Name).Returns(name);
        return this;
    }

    public RoleBuilder SetPermissions(GuildPermissions guildPermissions)
    {
        Mock.Setup(o => o.Permissions).Returns(guildPermissions);
        return this;
    }

    public RoleBuilder SetPosition(int position)
    {
        Mock.Setup(o => o.Position).Returns(position);
        return this;
    }
}
