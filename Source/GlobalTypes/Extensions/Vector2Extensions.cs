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
        public static float DistanceTo(this Vector2 a, Vector2 b) => Vector2.Distance(a, b);
        public static Vector2 ScalarProduct(this Vector2 a, float scalar) => new(a.Y * -scalar, a.X * scalar);
        public static Vector2 VectorProduct(this Vector2 a, Vector2 b) => new(a.Y * b.X, a.X * b.Y);

        public static Vector2 Floored(this Vector2 v) => Vector2.Floor(v);
        public static Vector2 Ceiled(this Vector2 v) => Vector2.Ceiling(v);
        public static Vector2 SignCeiled(this Vector2 v) => 
            new(
                (v.X > 0 ? v.X.Ceiled() : v.X < 0 ? v.X.Floored() : 0),
                (v.Y > 0 ? v.Y.Ceiled() : v.Y < 0 ? v.Y.Floored() : 0));
        public static Vector2 SignFloored (this Vector2 v) =>
            new(
                (v.X > 0 ? v.X.Floored() : v.X < 0 ? v.X.Ceiled() : 0), 
                (v.Y > 0 ? v.Y.Floored() : v.Y < 0 ? v.Y.Ceiled() : 0));
        
        public static Vector2 Rounded(this Vector2 v) => Vector2.Round(v);
        public static Vector2 IntCast(this Vector2 v) => new((int)v.X, (int)v.Y);

        public static Vector2 Abs(this Vector2 v) => new(v.AbsX(), v.AbsY());
        public static float AbsX(this Vector2 v) => Math.Abs(v.X);
        public static float AbsY(this Vector2 v) => Math.Abs(v.Y);
    }
}