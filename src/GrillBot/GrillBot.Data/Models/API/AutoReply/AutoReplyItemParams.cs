using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Common.Infrastructure;

namespace GrillBot.Data.Models.API.AutoReply
{
    public class AutoReplyItemParams : IApiObject
    {
        [Required]
        public string Template { get; set; }

        [Required]
        public string Reply { get; set; }

        public long Flags { get; set; }

        public Dictionary<string, string> SerializeForLog()
        {
            return new Dictionary<string, string>
            {
                { nameof(Template), Template },
                { nameof(Reply), Reply },
                { nameof(Flags), Flags.ToString() }
            };
        }
    }
}
