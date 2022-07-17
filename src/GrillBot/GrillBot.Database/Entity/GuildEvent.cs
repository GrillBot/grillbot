using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity;

[Table(nameof(GuildEvent))]
public class GuildEvent
{
    public string Id { get; set; } = null!;

    [StringLength(30)]
    public string GuildId { get; set; } = null!;

    [ForeignKey(nameof(GuildId))]
    public Guild Guild { get; set; } = null!;
    
    public DateTime From { get; set; }
    
    public DateTime To { get; set; }
}
