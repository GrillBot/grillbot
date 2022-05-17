using GrillBot.Data.Models.AuditLog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Data.Extensions;

public static class ControllerExtensions
{
    public static void SetApiRequestData(this Controller controller, object request)
    {
        var apiRequest = controller.HttpContext.RequestServices.GetRequiredService<ApiRequest>();

        apiRequest.SetParams(request);
    }
}
