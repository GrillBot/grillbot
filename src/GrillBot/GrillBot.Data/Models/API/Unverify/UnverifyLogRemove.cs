using System.Collections.Generic;

namespace GrillBot.Data.Models.API.Unverify;

public class UnverifyLogRemove
{
    public List<Role> ReturnedRoles { get; set; }
    public List<string> ReturnedChannelIds { get; set; }
    public bool FromWeb { get; set; }
}
