using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Data.Models.AuditLog;

public class MessageData
{
    public AuditUserInfo Author { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Content { get; set; }
    public List<EmbedInfo> Embeds { get; set; } 

    public MessageData() { }

    public MessageData(AuditUserInfo author, DateTime createdAt, string content, List<EmbedInfo> embeds)
    {
        Author = author;
        CreatedAt = createdAt;
        Content = string.IsNullOrEmpty(content) ? null : content;
        Embeds = embeds.Count == 0 ? null : embeds;
    }

    public MessageData(IUser author, DateTime createdAt, string content, IEnumerable<IEmbed> embeds)
        : this(new AuditUserInfo(author), createdAt, content, embeds.Select(o => new EmbedInfo(o)).ToList()) { }

    public MessageData(IMessage message)
        : this(message.Author, message.CreatedAt.LocalDateTime, message.Content, message.Embeds) { }
}
