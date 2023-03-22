﻿using GrillBot.Core.Infrastructure;
using GrillBot.Data.Models.AuditLog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Extensions;

public static class ControllerExtensions
{
    public static void StoreParameters(this Controller controller, IEnumerable<IDictionaryObject> apiObjects)
    {
        controller.HttpContext.RequestServices
            .GetRequiredService<ApiRequest>()
            .AddParameters(apiObjects);
    }
}
