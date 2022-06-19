using GrillBot.App.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using NSwag;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace GrillBot.App.Infrastructure.OpenApi;

public class ApiKeyAuthProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var ctx = (AspNetCoreOperationProcessorContext)context;
        var metadata = ctx?.ApiDescription?.ActionDescriptor?.EndpointMetadata;

        if (metadata == null)
            return true;

        if (metadata.OfType<AllowAnonymousAttribute>().Any() || metadata.OfType<AuthorizeAttribute>().Any())
            return true;

        if (!metadata.OfType<ApiKeyAuthAttribute>().Any())
            return true;

        context.OperationDescription.Operation.Security ??= new List<OpenApiSecurityRequirement>();
        context.OperationDescription.Operation.Security.Add(new OpenApiSecurityRequirement()
        {
            { "ApiKey", Enumerable.Empty<string>() }
        });

        return true;
    }
}
