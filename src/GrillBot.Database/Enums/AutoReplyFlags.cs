using System;

namespace GrillBot.Database.Enums;

[Flags]
public enum AutoReplyFlags
{
    None = 0,

    /// <summary>
    /// This reply is disabled.
    /// </summary>
    Disabled = 1,

    /// <summary>
    /// Case sensitive
    /// </summary>
    CaseSensitive = 2
}
