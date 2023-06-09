namespace GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;

public class DiffRequest<TType>
{
    public TType? Before { get; set; }
    public TType? After { get; set; }
}
