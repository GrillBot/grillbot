using GrillBot.Common.Infrastructure;

namespace GrillBot.Common.Extensions;

public static class ApiObjectExtensions
{
    public static void AddApiObject(this Dictionary<string, string?> destination, IApiObject? apiObject, string name)
    {
        apiObject?.SerializeForLog().ToList()
            .ForEach(o => destination.Add($"{name}.{o.Key}", o.Value));
    }
}
