using System;

namespace GrillBot.Database.Enums
{
    /// <summary>
    /// Command flags
    /// </summary>
    [Flags]
    public enum CommandFlags
    {
        None = 0,

        /// <summary>
        /// Command is disabled.
        /// </summary>
        Blocked = 1
    }
}
