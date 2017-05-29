using System;
using System.Collections.Generic;
using System.Threading;

namespace MongolianBarbecue.Tests.Extensions
{
    public static class RandomExtensions
    {
        static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random(DateTime.Now.GetHashCode()));

        public static TItem RandomItem<TItem>(this List<TItem> collection)
        {
            var index = Random.Value.Next(collection.Count);

            return collection[index];
        }
    }
}
