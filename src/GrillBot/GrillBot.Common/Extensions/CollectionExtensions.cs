namespace GrillBot.Common.Extensions;

public static class CollectionExtensions
{
    public static async Task<List<T>> FindAllAsync<T>(this IEnumerable<T> collection, Func<T, Task<bool>> func)
    {
        var result = new List<T>();

        foreach (var item in collection)
        {
            if (await func(item)) result.Add(item);
        }

        return result;
    }

    public static IEnumerable<IEnumerable<T>> SplitInParts<T>(this IEnumerable<T> source, int partSize)
    {
        var sourceData = source as List<T> ?? source.ToList();

        for (var i = 0; i < Math.Ceiling((double)sourceData.Count / partSize); i++)
            yield return sourceData.Skip(i * partSize).Take(partSize);
    }

    public static IEnumerable<TSource> Flatten<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>?> getChildren)
    {
        foreach (var item in source)
        {
            yield return item;

            var childrenData = getChildren(item);
            if (childrenData == null) yield break;

            foreach (var child in childrenData.Flatten(getChildren))
                yield return child;
        }
    }
}
