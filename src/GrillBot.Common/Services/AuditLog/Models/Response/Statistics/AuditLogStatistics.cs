﻿namespace GrillBot.Common.Services.AuditLog.Models.Response.Statistics;

public class AuditLogStatistics
{
    /// <summary>
    /// Statistics by type.
    /// Key is type name, value is count of records.
    /// </summary>
    public Dictionary<string, long> ByType { get; set; } = new();

    /// <summary>
    /// Statistics by date.
    /// Key is month and year, value is count of records.
    /// </summary>
    public Dictionary<string, long> ByDate { get; set; } = new();

    /// <summary>
    /// Statistics of stored files in the audit log.
    /// Key is file extension, value is count of files.
    /// </summary>
    public Dictionary<string, long> FileCounts { get; set; } = new();

    /// <summary>
    /// Statistics of stored files in the audit log.
    /// Key is file extension, value is size of files.
    /// </summary>
    public Dictionary<string, long> FileSizes { get; set; } = new();
}
