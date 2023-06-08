namespace GrillBot.Common.Services.AuditLog.Models;

public class DiffRequest<TType>
{
    public TType? Before { get; set; }
    public TType? After { get; set; }
}
