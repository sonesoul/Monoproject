using Microsoft.Xna.Framework;
using System;

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
        public static Vector2 Floored(this Vector2 v) 
        { 
            v.Floor(); 
            return v;
        }
        public static Vector2 Rounded(this Vector2 v)
        {
            v.Round();
            return v;
        }

        public static Vector2 Abs(this Vector2 v) => new(v.AbsX(), v.AbsY());
        public static float AbsX(this Vector2 v) => Math.Abs(v.X);
        public static float AbsY(this Vector2 v) => Math.Abs(v.Y);
    }
}
