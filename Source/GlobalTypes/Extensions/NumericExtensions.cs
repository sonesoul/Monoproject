using System;

namespace GlobalTypes.Extensions
{
    public static class NumericExtensions
    {
        #region Clamp
        public static float Clamp01(this float value) => Clamp(value, 0, 1);
        public static double Clamp01(this double value) => Clamp(value, 0, 1);
        public static int Clamp01(this int value) => Clamp(value, 0, 1);

        public static T ClampMin<T>(this T value, T min) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;

            return value;
        }
        public static T ClampMax<T>(this T value, T max) where T : IComparable<T>
        {
            if (value.CompareTo(max) > 0)
                return max;

            return value;
        }

        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            return value.ClampMin(min).ClampMax(max);
        }

        #endregion

        #region Abs
        public static int Abs(this int value) => Math.Abs(value);
        public static float Abs(this float value) => Math.Abs(value);
        #endregion

        #region Strings
        public static string AsSize(this long sizeInBytes)
        {
            ulong sizeBytes = (ulong)sizeInBytes;
            double sizeKb = sizeBytes / 1024.0;
            double sizeMb = sizeKb / 1024.0;
            double sizeGb = sizeMb / 1024.0;

            string finalSize;
            if (sizeGb >= 1)
                finalSize = $"{sizeGb:F2} GB";
            else if (sizeMb >= 1)
                finalSize = $"{sizeMb:F2} MB";
            else if (sizeKb >= 1)
                finalSize = $"{sizeKb:F2} KB";
            else
                finalSize = $"{sizeBytes} b";
            return finalSize.ToString();
        }
        public static string AsDifference<T>(this T value, T comparable) where T : IComparable<T>
        {
            if (value.CompareTo(comparable) >= 0)
            {
                return $"+{value}";
            }
            else
            {
                return $"-{value}";
            }
        }
        #endregion
    }
}