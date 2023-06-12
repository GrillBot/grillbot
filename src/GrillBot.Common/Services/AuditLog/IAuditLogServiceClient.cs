using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;
using GrillBot.Common.Services.AuditLog.Models.Request.Search;
using GrillBot.Common.Services.AuditLog.Models.Response;
using GrillBot.Common.Services.AuditLog.Models.Response.Detail;
using GrillBot.Common.Services.AuditLog.Models.Response.Search;
using GrillBot.Common.Services.Common;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Common.Services.AuditLog;

public interface IAuditLogServiceClient : IClient
{
    Task CreateItemsAsync(List<LogRequest> requests);
    Task<DiagnosticInfo> GetDiagAsync();
    Task<DeleteItemResponse> DeleteItemAsync(Guid id);
    Task<RestResponse<PaginatedResponse<LogListItem>>> SearchItemsAsync(SearchRequest request);
    Task<Detail?> DetailAsync(Guid id);
    Task<ArchivationResult?> ProcessArchivationAsync();
}
