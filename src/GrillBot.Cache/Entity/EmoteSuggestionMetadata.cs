using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Cache.Entity;

public class EmoteSuggestionMetadata
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; } = null!;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required]
    public byte[] DataContent { get; set; } = null!;

    [StringLength(200)]
    public string Filename { get; set; } = null!;
}
