using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalTypes.Extensions
{
    public static class EnumerableExtensions
    {
        private readonly static Random random = new();
        public static T RandomElement<T>(this IEnumerable<T> values) => values.ElementAt(random.Next(values.Count()));
        public static void PForEach<T>(this IEnumerable<T> values, Action<T> action) => Parallel.ForEach(values, action);
    }
}