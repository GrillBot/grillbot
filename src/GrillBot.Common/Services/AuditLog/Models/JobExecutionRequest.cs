using System.ComponentModel.DataAnnotations;

namespace AuditLogService.Models.Request;

public class JobExecutionRequest
{
    [Required]
    [StringLength(128)]
    public string JobName { get; set; } = null!;

    [Required]
    public string Result { get; set; } = null!;

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }

    [Required]
    public bool WasError { get; set; }

    [StringLength(32)]
    public string? StartUserId { get; set; }
}
