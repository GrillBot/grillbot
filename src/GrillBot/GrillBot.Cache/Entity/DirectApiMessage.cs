using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Cache.Entity;

public class DirectApiMessage
{
    [Key]
    [StringLength(30)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; } = null!;

    [Required]
    public DateTime ExpireAt { get; set; }

    [Required]
    public string JsonData { get; set; } = null!;
}
