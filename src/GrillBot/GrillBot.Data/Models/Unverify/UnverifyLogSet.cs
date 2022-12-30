using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.Unverify;

public class UnverifyLogSet
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<ulong> RolesToKeep { get; set; } = new();
    public List<ulong> RolesToRemove { get; set; } = new();
    public List<ChannelOverride> ChannelsToKeep { get; set; } = new();
    public List<ChannelOverride> ChannelsToRemove { get; set; } = new();
    public string Reason { get; set; }
    public bool IsSelfUnverify { get; set; }
    public string Language { get; set; }
    public bool KeepMutedRole { get; set; }

    public static UnverifyLogSet FromProfile(UnverifyUserProfile profile)
    {
        return new UnverifyLogSet
        {
            ChannelsToKeep = profile.ChannelsToKeep,
            ChannelsToRemove = profile.ChannelsToRemove,
            End = profile.End,
            Reason = profile.Reason,
            RolesToKeep = profile.RolesToKeep.ConvertAll(o => o.Id),
            RolesToRemove = profile.RolesToRemove.ConvertAll(o => o.Id),
            Start = profile.Start,
            IsSelfUnverify = profile.IsSelfUnverify,
            Language = profile.Language,
            KeepMutedRole = profile.KeepMutedRole
        };
    }
}
