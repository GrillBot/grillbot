using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Data.Models.API.Services;

public class ServiceInfo
{
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public int Timeout { get; set; }

    public string? ApiErrorMessage { get; set; }
    public DiagnosticInfo? DiagnosticInfo { get; set; }
}
