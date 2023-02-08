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
