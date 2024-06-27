using GrillBot.Common.Exceptions;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.AuditLog;
using Microsoft.AspNetCore.Http;

namespace GrillBot.App.Actions.Api.V3.Logging;

public class FrontendLogHandler : ApiAction
{
    private readonly LoggingManager _logging;

    public FrontendLogHandler(ApiRequestContext apiContext, LoggingManager logging) : base(apiContext)
    {
        _logging = logging;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = GetParameter<FrontendLogItemRequest>(0);

        switch (parameters.Severity)
        {
            case LogSeverity.Critical or LogSeverity.Error:
                await _logging.ErrorAsync(parameters.Source, "An error occured in the frontend.", new FrontendException(parameters.Message, ApiContext.LoggedUser!));
                break;
            case LogSeverity.Debug or LogSeverity.Info or LogSeverity.Verbose:
                await _logging.InfoAsync(parameters.Source, parameters.Message);
                break;
            case LogSeverity.Warning:
                await _logging.WarningAsync(parameters.Source, parameters.Message);
                break;
        }

        return new ApiResult(StatusCodes.Status201Created);
    }
}
