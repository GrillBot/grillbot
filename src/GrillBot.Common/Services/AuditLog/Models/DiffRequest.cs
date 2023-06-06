namespace AuditLogService.Models.Request;

public class DiffRequest<TType>
{
    public TType? Before { get; set; }
    public TType? After { get; set; }
}
