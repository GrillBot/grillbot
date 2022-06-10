namespace GrillBot.Database.Enums;

/// <summary>
/// Unverify operations.
/// </summary>
public enum UnverifyOperation
{
    /// <summary>
    /// Manual unverify (remove all possible roles and channels).
    /// </summary>
    Unverify,

    /// <summary>
    /// Selfunverify (remove all possible roles and channels), ignore keeped.
    /// </summary>
    Selfunverify,

    /// <summary>
    /// Automatic return access after unverify end.
    /// </summary>
    Autoremove,

    /// <summary>
    /// Manual return access.
    /// </summary>
    Remove,

    /// <summary>
    /// Unverify time updated.
    /// </summary>
    Update,

    /// <summary>
    /// Recover members access to state before unverify.
    /// </summary>
    Recover
}
