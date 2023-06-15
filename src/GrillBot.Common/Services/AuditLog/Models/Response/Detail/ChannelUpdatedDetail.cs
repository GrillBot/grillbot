using GrillBot.Core.Models;

namespace GrillBot.Common.Services.AuditLog.Models.Response.Detail;

public class ChannelUpdatedDetail
{
    public Diff<string?>? Name { get; set; }
    public Diff<int?>? SlowMode { get; set; }
    public Diff<bool?>? IsNsfw { get; set; }
    public Diff<int?>? Bitrate { get; set; }
    public Diff<string?>? Topic { get; set; }
    public Diff<int>? Position { get; set; }
    public Diff<int>? Flags { get; set; }
}
