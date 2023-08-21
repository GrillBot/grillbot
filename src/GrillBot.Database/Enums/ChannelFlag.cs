﻿using System;

namespace GrillBot.Database.Enums;

[Flags]
public enum ChannelFlag : long
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

    /// <summary>
    /// Channel or thread was deleted/archived.
    /// </summary>
    Deleted = 4,

    /// <summary>
    /// Automatic replies are disabled in this channel.
    /// </summary>
    AutoReplyDeactivated = 8,

    /// <summary>
    /// Points counting in the channel is disabled.
    /// </summary>
    PointsDeactivated = 16,

    /// <summary>
    /// Commands are executed as ephemeral.
    /// </summary>
    EphemeralCommands = 32
}
