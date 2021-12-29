namespace GrillBot.Data.Models.API.AutoReply
{
    public class AutoReplyItem
    {
        public long Id { get; set; }
        public string Template { get; set; }
        public string Reply { get; set; }
        public long Flags { get; set; }

        public AutoReplyItem() { }

        public AutoReplyItem(Database.Entity.AutoReplyItem item)
        {
            Id = item.Id;
            Template = item.Template;
            Reply = item.Reply;
            Flags = item.Flags;
        }
    }
}
