using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Data.Extensions
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

        public static IEnumerable<IEnumerable<T>> SplitInParts<T>(this IEnumerable<T> source, int partSize)
        {
            for (int i = 0; i < Math.Ceiling((double)source.Count() / partSize); i++)
                yield return source.Skip(i * partSize).Take(partSize);
        }
    }
}
