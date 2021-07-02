using System.Collections.Generic;

namespace GrillBot.Data.Models.Unverify
{
    public class UnverifyLogRemove
    {
        public List<ulong> ReturnedRoles { get; set; }
        public List<ChannelOverride> ReturnedOverwrites { get; set; }
    }
}
