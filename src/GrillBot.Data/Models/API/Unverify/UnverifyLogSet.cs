using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.API.Unverify;

public class UnverifyLogSet
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<Role> RolesToKeep { get; set; } = new();
    public List<Role> RolesToRemove { get; set; } = new();
    public List<string> ChannelIdsToKeep { get; set; } = new();
    public List<string> ChannelIdsToRemove { get; set; } = new();
    public string? Reason { get; set; }
    public bool IsSelfUnverify { get; set; }
    public string? Language { get; set; }
}
