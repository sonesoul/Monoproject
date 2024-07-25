namespace GlobalTypes.Extensions
{
    public static class FloatExtensions
    {
        public static float AsRadians(this float degrees) => HMath.DegToRad(degrees);
        public static float AsDegrees(this float radians) => HMath.RadToDeg(radians);
    }
}