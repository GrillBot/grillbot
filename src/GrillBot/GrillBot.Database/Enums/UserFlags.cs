using System;

namespace GrillBot.Database.Enums
{
    /// <summary>
    /// User specific flags.
    /// </summary>
    [Flags]
    public enum UserFlags
    {
        None = 0,

        /// <summary>
        /// User have full permissions.
        /// </summary>
        BotAdmin = 1,

        /// <summary>
        /// User have access to webadmin.
        /// </summary>
        WebAdmin = 2,

        /// <summary>
        /// User is not standard user.
        /// </summary>
        NotUser = 4
    }
}
