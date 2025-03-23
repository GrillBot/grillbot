using GrillBot.Common.Services.KachnaOnline.Models;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using Refit;

namespace GrillBot.Common.Services.KachnaOnline;

[Service("KachnaOnline")]
public interface IKachnaOnlineClient : IServiceClient
{
    [Get("/states/current")]
    Task<DuckState> GetCurrentStateAsync(CancellationToken cancellationToken = default);

    [Get("/states/next")]
    Task<DuckState?> GetNextStateAsync(Enums.DuckState type, CancellationToken cancellationToken = default);
}
