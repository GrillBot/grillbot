using System.Collections.Generic;

namespace GrillBot.Data.Models.Unverify;

public class UnverifyLogRemove
{
    public List<ulong> ReturnedRoles { get; set; } = new();
    public List<ChannelOverride> ReturnedOverwrites { get; set; } = new();
    public bool FromWeb { get; set; }
    public string Language { get; set; }
}
