using Discord;

namespace GrillBot.Data.Models.AuditLog
{
    public class MessageEditedData
    {
        public Diff<string> Diff { get; set; }
        public string JumpUrl { get; set; }

        public MessageEditedData() { }

        public MessageEditedData(string before, string after, string jumpUrl)
        {
            Diff = new Diff<string>(before, after);
            JumpUrl = jumpUrl;
        }

        public MessageEditedData(IMessage before, IMessage after)
            : this(before.Content, after.Content, after.GetJumpUrl()) { }
    }
}
