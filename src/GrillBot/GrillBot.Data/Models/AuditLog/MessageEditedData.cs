using Discord;

namespace GrillBot.Data.Models.AuditLog
{
    public class MessageEditedData
    {
        public string Before { get; set; }
        public string After { get; set; }
        public string JumpUrl { get; set; }

        public MessageEditedData() { }

        public MessageEditedData(string before, string after, string jumpUrl)
        {
            Before = before;
            After = after;
            JumpUrl = jumpUrl;
        }

        public MessageEditedData(IMessage before, IMessage after)
            : this(before.Content, after.Content, after.GetJumpUrl()) { }
    }
}
