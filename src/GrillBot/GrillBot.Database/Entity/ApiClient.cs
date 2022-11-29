using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity;

public class ApiClient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "jsonb")]
    public List<string> AllowedMethods { get; set; } = new();
    
    public int UseCount { get; set; }
    
    public DateTime LastUse { get; set; }
}
