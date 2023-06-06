using System.ComponentModel.DataAnnotations;

namespace AuditLogService.Models.Request;

public class InteractionCommandParameterRequest
{
    [Required]
    public string Name { get; set; } = null!;
    
    [Required]
    public string Type { get; set; } = null!;
    
    [Required]
    public string Value { get; set; } = null!;
}
