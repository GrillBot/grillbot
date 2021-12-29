using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Models.API.AutoReply
{
    public class AutoReplyItemParams
    {
        [Required]
        public string Template { get; set; }

        [Required]
        public string Reply { get; set; }

        public long Flags { get; set; }
    }
}
