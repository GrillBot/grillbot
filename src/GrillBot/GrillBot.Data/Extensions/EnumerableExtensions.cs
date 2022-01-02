using System;
using System.Collections.Generic;

namespace GrillBot.Data.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<TSource> Flatten<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> getChildren)
    {
        foreach (var item in source)
        {
            yield return item;

            foreach (var child in getChildren(item).Flatten(getChildren))
                yield return child;
        }
    }
}
