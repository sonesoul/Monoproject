using Microsoft.Xna.Framework;
using System;

namespace GlobalTypes.Extensions
{
    public static class Vector2Extensions
    {
        public static float Cross(this Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
        public static Vector2 Perpendicular(this Vector2 v) => new(v.Y, -v.X);
        public static Vector2 UnitNormal(this Vector2 v) => new Vector2(-v.Y, v.X).Normalized();
        public static Vector2 Normalized(this Vector2 v) => Vector2.Normalize(v);
        public static float DistanceTo(this Vector2 v, Vector2 other) => Vector2.Distance(v, other);

        public static Vector2 Floored(this Vector2 v) => Vector2.Floor(v);
        public static Vector2 Ceiled(this Vector2 v) => Vector2.Ceiling(v);
        public static Vector2 Rounded(this Vector2 v) => Vector2.Round(v);
        public static Vector2 IntCast(this Vector2 v) => new((int)v.X, (int)v.Y);

        public static Vector2 Abs(this Vector2 v) => new(v.AbsX(), v.AbsY());
        public static float AbsX(this Vector2 v) => Math.Abs(v.X);
        public static float AbsY(this Vector2 v) => Math.Abs(v.Y);
    }
}