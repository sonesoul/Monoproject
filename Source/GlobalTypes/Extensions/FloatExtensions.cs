using System;

namespace GlobalTypes.Extensions
{
    public static class FloatExtensions
    {
        public static float AsRad(this float degrees) => degrees * (float)Math.PI / 180;
        public static float AsDeg(this float radians) => radians * 180f / (float)Math.PI;
        
        public static float Floored(this float value) => (float)Math.Floor(value);
        public static float Ceiled(this float value) => (float)Math.Ceiling(value);
        public static float Rounded(this float value) => (float)Math.Round(value);
    }
}