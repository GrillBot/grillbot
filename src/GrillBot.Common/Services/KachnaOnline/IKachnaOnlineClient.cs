using GrillBot.Common.Services.KachnaOnline.Models;

namespace GrillBot.Common.Services.KachnaOnline;

public interface IKachnaOnlineClient
{
    Task<DuckState> GetCurrentStateAsync();

    Task<DuckState?> GetNextStateAsync(Enums.DuckState stateType);
}
