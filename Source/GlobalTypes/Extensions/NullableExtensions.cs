using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalTypes.Extensions
{
    public static class NullableExtensions
    {
        public static bool TryGetValue<T>(this T? n, out T value) where T : struct 
        {
            value = default;

            if (n.HasValue)
            {
                value = n.Value;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
