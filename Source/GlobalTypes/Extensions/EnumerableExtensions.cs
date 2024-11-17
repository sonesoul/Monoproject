using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalTypes.Extensions
{
    public static class EnumerableExtensions
    {
        private readonly static Random random = new();
        public static T RandomElement<T>(this IEnumerable<T> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values), "Collection is null.");
            }

            if (!values.Any())
                throw new Exception("There are no items that can be obtained randomly.");

            return values.ElementAt(random.Next(values.Count()));
        }
        public static void PForEach<T>(this IEnumerable<T> values, Action<T> action) => Parallel.ForEach(values, action);

        public static void For<T>(this IEnumerable<T> values, Action<T> action)
        {
            List<T> snapshot = values.ToList();

            for (var i = 0; i < snapshot.Count; i++)
            {
                action(snapshot[i]);
            }
        }
    }
}