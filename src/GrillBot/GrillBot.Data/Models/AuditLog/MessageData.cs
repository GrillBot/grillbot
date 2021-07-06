using Discord;
using System;

namespace GrillBot.Data.Models.AuditLog
{
    public class MessageData
    {
        public AuditUserInfo Author { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Content { get; set; }

        public MessageData() { }

        public MessageData(AuditUserInfo author, DateTime createdAt, string content)
        {
            Author = author;
            CreatedAt = createdAt;
            Content = string.IsNullOrEmpty(content) ? null : content;
        }

        public MessageData(IUser author, DateTime createdAt, string content)
            : this(new AuditUserInfo(author), createdAt, content) { }

        public MessageData(IUserMessage message)
            : this(message.Author, message.CreatedAt.LocalDateTime, message.Content) { }
    }
}
