using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Common.Infrastructure;

namespace GrillBot.Data.Models.API.Guilds.GuildEvents;

/// <summary>
/// Event definition.
/// </summary>
public class ScheduledEventParams : IApiObject
{
    /// <summary>
    /// The name of the event.
    /// Required for new events.
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; }

    /// <summary>
    /// The start time of the event.
    /// Required for new events, otherwise if value not changed set <see cref="DateTime.MinValue"/>.
    /// </summary>
    public DateTime StartAt { get; set; }

    /// <summary>
    /// End time of the event.
    /// Required for new events, otherwise if value not changed set <see cref="DateTime.MinValue"/>.
    /// </summary>
    public DateTime EndAt { get; set; }

    /// <summary>
    /// Description of the event.
    /// </summary>
    [StringLength(1000, MinimumLength = 1)]
    public string Description { get; set; }

    /// <summary>
    /// Location of the event.
    /// Required for new events.
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    public string Location { get; set; }

    /// <summary>
    /// Image of the event as Base64 image.
    /// </summary>
    public byte[] Image { get; set; }

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(Name), Name },
            { nameof(StartAt), StartAt.ToString("o") },
            { nameof(EndAt), EndAt.ToString("o") },
            { nameof(Description), Description },
            { nameof(Location), Location },
            { "ImageLength", Image?.Length.ToString() ?? "" }
        };
    }
}
