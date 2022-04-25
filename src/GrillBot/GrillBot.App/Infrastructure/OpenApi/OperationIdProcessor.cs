using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace GrillBot.App.Infrastructure.OpenApi;

public class OperationIdProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var controller = context.ControllerType.Name;
        var action = context.MethodInfo.Name;

        context.OperationDescription.Operation.OperationId = $"{controller}_{action}";

        return true;
    }
}
