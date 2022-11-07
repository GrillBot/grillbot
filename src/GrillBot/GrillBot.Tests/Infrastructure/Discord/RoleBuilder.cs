using Discord;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class RoleBuilder : BuilderBase<IRole>
{
    public RoleBuilder(ulong id, string name)
    {
        SetId(id);
        SetName(name);
    }

    public RoleBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        Mock.Setup(o => o.Mention).Returns($"<#{id}>");
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

    public RoleBuilder SetTags(RoleTags tags)
    {
        Mock.Setup(o => o.Tags).Returns(tags);
        return this;
    }
}
