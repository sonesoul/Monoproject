using Microsoft.Xna.Framework;

namespace Extensions.Extensions
{
    public static class Vector2Extensions
    {
        public static float Cross(this Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
    }
}
