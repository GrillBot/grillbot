using System.Text.RegularExpressions;

namespace GrillBot.Data.Models
{
    public class AutoReplyConfiguration
    {
        public string Template { get; set; }
        public string Reply { get; set; }
        public bool Disabled { get; set; }
        public bool CaseSensitive { get; set; }

        public RegexOptions Options => RegexOptions.Multiline | (CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
    }
}
