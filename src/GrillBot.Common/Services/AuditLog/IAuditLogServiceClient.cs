using GrillBot.Common.Services.AuditLog.Models;
using GrillBot.Common.Services.AuditLog.Models.Response;
using GrillBot.Common.Services.Common;
using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Common.Services.AuditLog;

public interface IAuditLogServiceClient : IClient
{
    Task CreateItemsAsync(List<LogRequest> requests);
    Task<DiagnosticInfo> GetDiagAsync();
    Task<DeleteItemResponse> DeleteItemAsync(Guid id);
}
