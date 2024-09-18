using System;

namespace GlobalTypes.Extensions
{
    public static class NumericExtensions
    {
        #region Clamp
        public static int Clamp(this int value, int min, int max) 
            => value < min ? min : value > max ? max : value;
        public static float Clamp(this float value, float min, float max) 
            => value < min ? min : value > max ? max : value;
        #endregion

        #region Abs
        public static int Abs(this int value) => Math.Abs(value);
        public static float Abs(this float value) => Math.Abs(value);
        #endregion

        #region ToSizeString
        public static string ToSizeString(this long sizeInBytes)
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
        #endregion
    }
}
