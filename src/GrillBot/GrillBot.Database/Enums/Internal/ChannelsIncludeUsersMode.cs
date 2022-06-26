namespace GrillBot.Database.Enums.Internal;

public enum ChannelsIncludeUsersMode
{
    /// <summary>
    /// Do not include Users.
    /// </summary>
    None = 0,

    /// <summary>
    /// Include all users except bots and inactive.
    /// </summary>
    IncludeExceptInactive = 1,
    
    /// <summary>
    /// Include all users.
    /// </summary>
    IncludeAll = 2
}
