using System;

namespace GrillBot.Database.Enums;

[Flags]
public enum ChannelFlags
{
    None = 0,

    /// <summary>
    /// Channel is hidden from statistics.
    /// </summary>
    StatsHidden = 1
}
