using System;

namespace GlobalTypes.Extensions
{
    public static class FloatExtensions
    {
        public static float AsRad(this float degrees) => HMath.DegToRad(degrees);
        public static float AsDeg(this float radians) => HMath.RadToDeg(radians);
        
        public static float Floored(this float value) => (float)Math.Floor(value);
        public static float Ceiled(this float value) => (float)Math.Ceiling(value);
        public static float Rounded(this float value) => (float)Math.Round(value);
       
        public static float Abs(this float value) => Math.Abs(value); 
    }
}