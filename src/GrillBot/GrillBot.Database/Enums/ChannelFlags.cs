using System;

namespace GrillBot.Database.Enums;

[Flags]
public enum ChannelFlags
{
    None = 0,

    /// <summary>
    /// Channel is hidden from statistics.
    /// </summary>
    StatsHidden = 1,

    /// <summary>
    /// Commands execution is disabled in this channel.
    /// </summary>
    CommandsDisabled = 2,
}
