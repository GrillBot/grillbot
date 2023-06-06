using System.ComponentModel.DataAnnotations;

namespace AuditLogService.Models.Request;

public class FileRequest
{
    [Required]
    [StringLength(255)]
    public string Filename { get; set; } = null!;

    [StringLength(255)]
    public string? Extension { get; set; }

    [Range(0, long.MaxValue)]
    public long Size { get; set; }
}
