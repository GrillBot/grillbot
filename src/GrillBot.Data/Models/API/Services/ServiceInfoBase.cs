namespace GrillBot.Data.Models.API.Services;

public abstract class ServiceInfoBase
{
    public string Url { get; set; } = null!;
    public int Timeout { get; set; }
    
    public string? ApiErrorMessage { get; set; }
}
