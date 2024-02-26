namespace GrillBot.Common.Extensions;

public static class EnumerableExtensions
{
    public static bool IsSequenceEqual<TSource, TKey>(
        this IEnumerable<TSource> source,
        IEnumerable<TSource> second,
        Func<TSource, TKey> sort
    ) => source.OrderBy(sort).SequenceEqual(second.OrderBy(sort));
}
