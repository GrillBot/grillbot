using Discord;

namespace GrillBot.Data.Models.AuditLog;

public class MessageDeletedData
{
    public bool Cached { get; set; }
    public MessageData Data { get; set; }

    public MessageDeletedData() { }

    public MessageDeletedData(bool cached)
    {
        Cached = cached;
    }

    public MessageDeletedData(MessageData data) : this(data != null)
    {
        Data = data;
    }

    public MessageDeletedData(IUserMessage message) : this(message != null ? new MessageData(message) : null) { }
}
