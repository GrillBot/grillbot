using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace GrillBot.Database.Entity
{
    public class AuditLogFileMeta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long AuditLogItemId { get; set; }

        [ForeignKey(nameof(AuditLogItemId))]
        public AuditLogItem AuditLogItem { get; set; }

        [StringLength(255)]
        public string Filename { get; set; }

        [NotMapped]
        public string Extension => Path.GetFileNameWithoutExtension(Filename);

        [Required]
        public long Size { get; set; } = 0;

        public byte[] ReadContent(string rootPath)
        {
            return File.ReadAllBytes(Path.Combine(rootPath, Filename));
        }

        public StreamReader CreateStream(string rootPath)
        {
            return new StreamReader(Path.Combine(rootPath, Filename));
        }
    }
}
