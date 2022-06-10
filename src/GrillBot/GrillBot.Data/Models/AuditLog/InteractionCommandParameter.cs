using Discord;
using Discord.WebSocket;

namespace GrillBot.Data.Models.AuditLog;

public class InteractionCommandParameter
{
    public string Name { get; set; }
    public string Type { get; set; }
    public object Value { get; set; }

    public InteractionCommandParameter() { }

    public InteractionCommandParameter(IApplicationCommandInteractionDataOption option)
    {
        Name = option.Name;
        Type = option.Type.ToString();

        switch (Type)
        {
            case "Boolean":
            case "Integer":
            case "Number":
            case "String":
                Value = option.Value;
                break;
            case "Channel":
                Value = new AuditChannelInfo(option.Value as IChannel);
                break;
            case "Role":
                Value = new AuditRoleInfo(option.Value as IRole);
                break;
            case "User":
                Value = new AuditUserInfo(option.Value as IUser);
                break;
        }
    }

    public InteractionCommandParameter(SocketUserCommandData data)
    {
        Name = data.Name;
        Type = "User";
        Value = new AuditUserInfo(data.Member);
    }

    public InteractionCommandParameter(SocketMessageCommandData data)
    {
        Name = data.Name;
        Type = "Message";
        Value = new MessageData(data.Message.Author, data.Message.CreatedAt.LocalDateTime, data.Message.Content);
    }
}
