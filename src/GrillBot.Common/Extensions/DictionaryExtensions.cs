using GrillBot.Core.Infrastructure;

namespace GrillBot.Common.Extensions;

public static class DictionaryExtensions
{
    public static void MergeDictionaryObjects(this Dictionary<string, string?> destination, IDictionaryObject? @object, string name)
    {
        @object?.ToDictionary().ToList()
            .ForEach(o => destination.Add($"{name}.{o.Key}", o.Value));
    }
}
