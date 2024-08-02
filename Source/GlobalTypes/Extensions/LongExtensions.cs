using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalTypes.Extensions
{
    public static class LongExtensions
    {
        public static string SizeString(this long sizeInBytes)
        {
            ulong sizeBytes = (ulong)sizeInBytes;
            double sizeKb = sizeBytes / 1024.0;
            double sizeMb = sizeKb / 1024.0;
            double sizeGb = sizeMb / 1024.0;

            string finalSize;
            if (sizeGb >= 1)
                finalSize = $"{sizeGb:F4} GB";
            else if (sizeMb >= 1)
                finalSize = $"{sizeMb:F4} MB";
            else if (sizeKb >= 1)
                finalSize = $"{sizeKb:F4} KB";
            else
                finalSize = $"{sizeBytes} b";
            return finalSize.ToString();
        }
    }
}
