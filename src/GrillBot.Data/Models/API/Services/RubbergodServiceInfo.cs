using GrillBot.Common.Services.RubbergodService.Models.Diagnostics;

namespace GrillBot.Data.Models.API.Services;

public class RubbergodServiceInfo : ServiceInfoBase
{
    public DiagnosticInfo Info { get; set; } = null!;
}
