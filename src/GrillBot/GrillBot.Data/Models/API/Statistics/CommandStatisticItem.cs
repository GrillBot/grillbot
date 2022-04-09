﻿using System;

namespace GrillBot.Data.Models.API.Statistics;

/// <summary>
/// Statistics about command.
/// </summary>
public class CommandStatisticItem
{
    /// <summary>
    /// Command
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    /// Last call of command
    /// </summary>
    public DateTime LastCall { get; set; }

    /// <summary>
    /// Count of success calls.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Count of failed count
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Minimal execution time.
    /// </summary>
    public int MinDuration { get; set; }

    /// <summary>
    /// Maximal execution time.
    /// </summary>
    public int MaxDuration { get; set; }

    /// <summary>
    /// Total duration time.
    /// </summary>
    public int TotalDuration { get; set; }

    /// <summary>
    /// Rate of success.
    /// </summary>
    public int SuccessRate
    {
        get
        {
            var sum = SuccessCount + FailedCount;
            return sum == 0 ? 0 : (int)Math.Round(((double)SuccessCount / sum) * 100);
        }
    }

    /// <summary>
    /// Average duration time.
    /// </summary>
    public int AvgDuration
    {
        get
        {
            var sum = SuccessCount + FailedCount;
            if (sum == 0) return 0;

            return Convert.ToInt32(Math.Round(TotalDuration / (double)sum));
        }
    }
}
