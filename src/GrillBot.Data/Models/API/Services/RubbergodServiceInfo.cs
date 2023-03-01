using GrillBot.Common.Services.Common.Models.Diagnostics;

namespace GrillBot.Data.Models.API.Services;

public class RubbergodServiceInfo : ServiceInfoBase
{
    public DiagnosticInfo Info { get; set; } = null!;
}
