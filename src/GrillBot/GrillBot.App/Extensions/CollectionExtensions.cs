using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrillBot.App.Extensions
{
    static public class CollectionExtensions
    {
        static public async Task<List<T>> FindAllAsync<T>(this IEnumerable<T> collection, Func<T, Task<bool>> func)
        {
            var result = new List<T>();

            foreach (var item in collection)
            {
                if (await func(item)) result.Add(item);
            }

            return result;
        }
    }
}
