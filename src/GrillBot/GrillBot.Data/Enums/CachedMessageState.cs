namespace GrillBot.Data.Enums
{
    public enum CachedMessageState
    {
        /// <summary>
        /// Nothing.
        /// </summary>
        None = 0,

        /// <summary>
        /// Message will be deleted at next cycle.
        /// </summary>
        ToBeDeleted = 1,

        /// <summary>
        /// Message needs update from Discord API.
        /// </summary>
        NeedsUpdate = 2
    }
}
