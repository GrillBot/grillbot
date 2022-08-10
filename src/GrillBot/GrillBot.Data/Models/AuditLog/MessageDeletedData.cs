using Discord;

namespace GrillBot.Data.Models.AuditLog;

public class MessageDeletedData
{
    public MessageData Data { get; set; }

    public MessageDeletedData() { }

    public MessageDeletedData(MessageData data)
    {
        Data = data;
    }

    public MessageDeletedData(IMessage message) : this(message != null ? new MessageData(message) : null) { }
}
