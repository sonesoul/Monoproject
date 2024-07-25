using Microsoft.Xna.Framework;

namespace GlobalTypes.Extensions
{
    public static class Vector2Extensions
    {
        public static float Cross(this Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
        public static Vector2 Perpendicular(this Vector2 v) => new(v.Y, -v.X);
        public static Vector2 Normalized(this Vector2 v)
        {
            v.Normalize();
            return v;
        }
    }
}
