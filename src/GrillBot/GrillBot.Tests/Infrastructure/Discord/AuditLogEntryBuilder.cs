using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

public class AuditLogEntryBuilder : BuilderBase<IAuditLogEntry>
{
    public AuditLogEntryBuilder(ulong id)
    {
        SetId(id);
    }

    public AuditLogEntryBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public AuditLogEntryBuilder SetData(IAuditLogData data)
    {
        Mock.Setup(o => o.Data).Returns(data);
        return this;
    }

    public AuditLogEntryBuilder SetUser(IUser user)
    {
        Mock.Setup(o => o.User).Returns(user);
        return this;
    }

    public AuditLogEntryBuilder SetActionType(ActionType actionType)
    {
        Mock.Setup(o => o.Action).Returns(actionType);
        return this;
    }
}
