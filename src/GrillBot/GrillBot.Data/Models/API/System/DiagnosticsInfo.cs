using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GrillBot.Data.Models.API.System;

/// <summary>
/// Diagnostics info
/// </summary>
public class DiagnosticsInfo
{
    /// <summary>
    /// Instance type (Release, Development, ...)
    /// </summary>
    public string InstanceType { get; set; }

    /// <summary>
    /// Datetime of start.
    /// </summary>
    public DateTime StartAt { get; set; }

    /// <summary>
    /// Uptime
    /// </summary>
    public TimeSpan Uptime { get; set; }

    /// <summary>
    /// CPU active time
    /// </summary>
    public TimeSpan CpuTime { get; set; }

    /// <summary>
    /// Connection state to discord.
    /// </summary>
    public ConnectionState ConnectionState { get; set; }

    /// <summary>
    /// Used memory in bytes.
    /// </summary>
    public long UsedMemory { get; set; }

    /// <summary>
    /// Bot is initialized and listening.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Current datetime on server.
    /// </summary>
    public DateTime CurrentDateTime { get; set; }

    public Dictionary<string, int> ActiveOperations { get; set; }

    public DiagnosticsInfo()
    {
        var process = Process.GetCurrentProcess();

        StartAt = process.StartTime;
        Uptime = DateTime.Now - process.StartTime;
        CpuTime = process.TotalProcessorTime;
        UsedMemory = process.WorkingSet64;
        CurrentDateTime = DateTime.Now;
    }

    public DiagnosticsInfo(string environmentName, ConnectionState connectionState, bool isActive, Dictionary<string, int> activeOperations) : this()
    {
        InstanceType = environmentName;
        ConnectionState = connectionState;
        IsActive = isActive;
        ActiveOperations = activeOperations;
    }
}
