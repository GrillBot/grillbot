using System;

namespace GrillBot.Data.Models.API.Unverify;

public class UnverifyInfo
{
    /// <summary>
    /// Start of unverify.
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// End of unverify.
    /// </summary>
    public DateTime End { get; set; }
    
    /// <summary>
    /// End to
    /// </summary>
    public TimeSpan EndTo { get; set; }
    
    /// <summary>
    /// Reason of remove.
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// Is this unverify selfunverify?
    /// </summary>
    public bool IsSelfUnverify { get; set; }
}
