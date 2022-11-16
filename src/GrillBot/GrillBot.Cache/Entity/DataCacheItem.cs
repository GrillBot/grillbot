using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Cache.Entity;

public class DataCacheItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Key { get; set; } = null!;

    [Required]
    public string Value { get; set; } = null!;
    
    [Required]
    public DateTime ValidTo { get; set; }
}
