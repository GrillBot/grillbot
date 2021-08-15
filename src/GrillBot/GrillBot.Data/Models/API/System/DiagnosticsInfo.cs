using Discord;
using Discord.WebSocket;
using System;
using System.Diagnostics;

namespace GrillBot.Data.Models.API.System
{
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
        /// Discord communication latency
        /// </summary>
        public TimeSpan Latency { get; set; }

        /// <summary>
        /// Connection state to discord.
        /// </summary>
        public ConnectionState ConnectionState { get; set; }

        /// <summary>
        /// Used memory in bytes.
        /// </summary>
        public long UsedMemory { get; set; }

        /// <summary>
        /// Status of bot account.
        /// </summary>
        public UserStatus UserStatus { get; set; }

        public DiagnosticsInfo()
        {
            var process = Process.GetCurrentProcess();

            StartAt = process.StartTime;
            Uptime = DateTime.Now - process.StartTime;
            CpuTime = process.TotalProcessorTime;
            UsedMemory = process.WorkingSet64;
        }

        public DiagnosticsInfo(string environmentName, DiscordSocketClient discord) : this()
        {
            InstanceType = environmentName;
            Latency = TimeSpan.FromMilliseconds(discord.Latency);
            ConnectionState = discord.ConnectionState;
            UserStatus = discord.Status;
        }
    }
}
