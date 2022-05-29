using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace GrillBot.Database.Entity;

public class AuditLogFileMeta
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long AuditLogItemId { get; set; }

    [ForeignKey(nameof(AuditLogItemId))]
    public AuditLogItem AuditLogItem { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Filename { get; set; } = null!;

    [NotMapped]
    public string Extension => Path.GetExtension(Filename);

    [NotMapped]
    public string FilenameWithoutExtension => Path.GetFileNameWithoutExtension(Filename);

    [Required]
    public long Size { get; set; } = 0;
}
