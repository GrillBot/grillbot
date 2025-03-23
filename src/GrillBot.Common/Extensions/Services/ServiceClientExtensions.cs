using GrillBot.Core.Services.Common;

namespace GrillBot.Common.Extensions.Services;

public static class ServiceClientExtensions
{
    public static string? GetServiceUrl(this IServiceClient client)
    {
        var clientType = client.GetType();
        var httpClientProperty = clientType.GetProperty("Client");

        if (httpClientProperty is null || httpClientProperty.GetValue(client) is not HttpClient httpClient)
            return null;
        return httpClient.BaseAddress?.ToString();
    }
}