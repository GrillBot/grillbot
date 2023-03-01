namespace GrillBot.Data.Models.API.AuditLog;

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
    
    /// <summary>
    /// Generated SAS LINK. Only for files stored in the AzureBlobStorage.
    /// </summary>
    public string? SasLink { get; set; }
}

public class AuditLogFileMetadataMappingProfile : AutoMapper.Profile
{
    public AuditLogFileMetadataMappingProfile()
    {
        CreateMap<Database.Entity.AuditLogFileMeta, AuditLogFileMetadata>();
    }
}
