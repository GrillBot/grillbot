using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;
using GrillBot.Core.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace GrillBot.Common.Extensions.AuditLog;

public static class ApiRequestRequestExtensions
{
    public static void AddParameter(this ApiRequestRequest request, string? name, string? value)
    {
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
            request.Parameters.Add(name, value);
    }

    private static void AddHeader(this ApiRequestRequest request, string? name, string? value)
    {
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
            request.Headers.Add(name, value);
    }

    public static void AddHeaders(this ApiRequestRequest request, string? name, StringValues values)
    {
        foreach (var value in values)
            request.AddHeader(name, value);
    }

    public static void AddParameters(this ApiRequestRequest request, IDictionaryObject? apiObject, int index = -1)
    {
        if (apiObject is null)
            return;

        foreach (var item in apiObject.ToDictionary())
            request.AddParameter(item.Key + (index > -1 ? $"[{index}]" : ""), item.Value);
    }
}
