using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Common.Models;

namespace GrillBot.App.Services.AuditLog;

public class AuditLogApiService
{
    private ApiRequestContext ApiRequestContext { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public AuditLogApiService(ApiRequestContext apiRequestContext, AuditLogWriter auditLogWriter)
    {
        ApiRequestContext = apiRequestContext;
        AuditLogWriter = auditLogWriter;
    }

    public async Task HandleClientAppMessageAsync(ClientLogItemRequest request)
    {
        var item = new AuditLogDataWrapper(request.GetAuditLogType(), request.Content, processedUser: ApiRequestContext.LoggedUser);
        await AuditLogWriter.StoreAsync(item);
    }
}
