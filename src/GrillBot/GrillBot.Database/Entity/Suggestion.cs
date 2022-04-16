using GrillBot.Database.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity;

public class Suggestion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public SuggestionType Type { get; set; }

    [Required]
    public string Data { get; set; }

    [Required]
    public string GuildId { get; set; }

    public byte[] BinaryData { get; set; }

    public string BinaryDataFilename { get; set; }
}
