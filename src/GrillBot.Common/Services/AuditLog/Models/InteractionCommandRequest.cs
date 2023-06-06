using System.ComponentModel.DataAnnotations;

namespace AuditLogService.Models.Request;

public class InteractionCommandRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string ModuleName { get; set; } = null!;

    [Required]
    public string MethodName { get; set; } = null!;

    public List<InteractionCommandParameterRequest> Parameters { get; set; } = new();

    [Required]
    public bool HasResponded { get; set; }

    [Required]
    public bool IsValidToken { get; set; }

    [Required]
    public bool IsSuccess { get; set; }

    public int? CommandError { get; set; }
    public string? ErrorReason { get; set; }

    [Required]
    public int Duration { get; set; }

    public string? Exception { get; set; }
    public string Locale { get; set; } = "cs";
}
