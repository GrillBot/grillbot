using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace GrillBot.App.Infrastructure.OpenApi;

public class OperationIdProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        context.OperationDescription.Operation.OperationId = $"{context.ControllerType.Name}_{context.MethodInfo.Name}";
        return true;
    }
}
