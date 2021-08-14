namespace GrillBot.Data.Models.API.AuditLog
{
    /// <summary>
    /// Metadata for files stored on disk.
    /// </summary>
    public class AuditLogFileMetadata
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Filename
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Size
        /// </summary>
        public long Size { get; set; }

        public AuditLogFileMetadata() { }

        public AuditLogFileMetadata(Database.Entity.AuditLogFileMeta file)
        {
            Id = file.Id;
            Filename = file.Filename;
            Size = file.Size;
        }
    }
}
