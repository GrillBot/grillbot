using System;
using System.ComponentModel.DataAnnotations;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.Guilds;

public class GuildEvent
{
    [Required]
    public string Id { get; set; }
    
    [Required]
    public RangeParams<DateTime> Validity { get; set; }
}
