using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.Generation.AspNetCore;

namespace GrillBot.App.Infrastructure.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiDoc(this IServiceCollection services, string version, string name, string description, Action<AspNetCoreOpenApiDocumentGeneratorSettings> setSecurity)
    {
        return services.AddOpenApiDocument(doc =>
        {
            doc.Version = version;
            doc.ApiGroupNames = new[] { version };
            doc.DocumentName = version;

            setSecurity(doc);

            doc.OperationProcessors.Add(new OnlyDevelopmentProcessor());
            doc.UseRouteNameAsOperationId = true;
            doc.UseControllerSummaryAsTagDescription = true;

            doc.PostProcess = document =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "GrillBot API",
                    Description = description,
                    Version = version + " - " + name,

                    License = new OpenApiLicense
                    {
                        Name = "All rights reserved",
                        Url = "https://gist.github.com/Techcable/e7bbc22ecbc0050efbcc"
                    }
                };
            };
        });
    }
}
