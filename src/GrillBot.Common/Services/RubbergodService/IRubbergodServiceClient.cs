using GrillBot.Common.Models.Pagination;
using GrillBot.Common.Services.Common.Models.Diagnostics;
using GrillBot.Common.Services.RubbergodService.Models.DirectApi;
using GrillBot.Common.Services.RubbergodService.Models.Karma;

namespace GrillBot.Common.Services.RubbergodService;

public interface IRubbergodServiceClient
{
    int Timeout { get; }
    string Url { get; }

    Task<bool> IsAvailableAsync();
    Task<DiagnosticInfo> GetDiagAsync();
    Task RefreshMemberAsync(ulong memberId);
    Task<string> SendDirectApiCommand(string service, DirectApiCommand command);
    Task<PaginatedResponse<UserKarma>> GetKarmaPageAsync(PaginatedParams parameters);
    Task StoreKarmaAsync(List<KarmaItem> items);
}
