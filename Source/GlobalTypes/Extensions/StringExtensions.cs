using System.Linq;

namespace GlobalTypes.Extensions
{
    public static class StringExtensions
    {
        public static string Times(this string str, int count) => string.Concat(Enumerable.Repeat(str, count));
        public static (string first, string second) Partition(this string str, int index) 
            => (str[..index], str[index..]);
        public static (string first, string second) Partition(this string str, char divider) 
            => str.Partition(str.IndexOf(divider));
        public static void Partition(this string str, int index, out string first, out string second) 
            => (first, second) = str.Partition(index);
        public static void Partition(this string str, char divider, out string first, out string second) 
            => (first, second) = str.Partition(divider);
    }
}