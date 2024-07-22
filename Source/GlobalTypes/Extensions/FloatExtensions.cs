namespace GlobalTypes.Extensions
{
    public static class FloatExtensions
    {
        public static float ToRad(this float degrees) => HMath.DegToRad(degrees);
        public static float ToDeg(this float radians) => HMath.RadToDeg(radians);
    }
}
