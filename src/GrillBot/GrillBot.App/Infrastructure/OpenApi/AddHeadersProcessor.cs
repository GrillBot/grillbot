using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace GrillBot.App.Infrastructure.OpenApi;

public class AddHeadersProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        context.OperationDescription.Operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Language",
            Kind = OpenApiParameterKind.Header,
            Type = NJsonSchema.JsonObjectType.String,
            IsRequired = false,
            Default = "cs",
            Description = "Allowed values: cs, en-US"
        });

        return true;
    }
}
