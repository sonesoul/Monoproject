using Microsoft.Xna.Framework;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GlobalTypes.Extensions
{
    public static class Vector2Extensions
    {
        public struct BothAxes
        {
            Vector2 vector;
            public readonly float X => vector.X;
            public readonly float Y => vector.Y;

            public BothAxes(float x, float y) => vector = new(x, y);
            public BothAxes(Vector2 vector) => this.vector = vector;

            public readonly override bool Equals([NotNullWhen(true)] object obj) => (obj is float axis) ? Equals(this, axis) : base.Equals(obj);
            private static bool Equals(BothAxes vector, float value) => vector.X == value && vector.Y == value;
            public readonly override int GetHashCode() => HashCode.Combine(vector.X, vector.Y);

            public static Vector2 operator +(BothAxes vector, float value) => new(vector.X + value, vector.Y + value);
            public static Vector2 operator -(BothAxes vector, float value) => new(vector.X - value, vector.Y - value);
            public static Vector2 operator *(BothAxes vector, float value) => new(vector.X * value, vector.Y * value);
            public static Vector2 operator /(BothAxes vector, float value) => new(vector.X / value, vector.Y / value);
            public static bool operator <(BothAxes vector, float value) => (vector.X < value && vector.Y < value);
            public static bool operator >(BothAxes vector, float value) => (vector.X > value && vector.Y > value);
            public static bool operator <=(BothAxes vector, float value) => vector < value || vector == value;
            public static bool operator >=(BothAxes vector, float value) => vector > value || vector == value;
            public static bool operator ==(BothAxes vector, float value) => vector.Equals(value);
            public static bool operator !=(BothAxes vector, float value) => !vector.Equals(value);
        }
        public struct AnyAxis
        {
            Vector2 vector;
            public readonly float X => vector.X;
            public readonly float Y => vector.Y;

            public AnyAxis(float x, float y) => vector = new(x, y);
            public AnyAxis(Vector2 vector) => this.vector = vector;

            public readonly override bool Equals([NotNullWhen(true)] object obj) => (obj is float axis) ? Equals(this, axis) : base.Equals(obj);
            private static bool Equals(AnyAxis left, float value) => (left.X == value || left.Y == value);

            public static bool operator >(AnyAxis vector, float value) => (vector.X > value || vector.Y > value);
            public static bool operator <(AnyAxis vector, float value) => (vector.X < value || vector.Y < value);
            public static bool operator <=(AnyAxis vector, float value) => vector < value || vector == value;
            public static bool operator >=(AnyAxis vector, float value) => vector > value || vector == value;
            public static bool operator ==(AnyAxis vector, float value) => vector.Equals(value);
            public static bool operator !=(AnyAxis vector, float value) => !vector.Equals(value);

            public readonly override int GetHashCode() => HashCode.Combine(vector.X, vector.Y);
        }

        public static float Cross(this Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
        public static Vector2 Perpendicular(this Vector2 v) => new(v.Y, -v.X);
        public static Vector2 UnitNormal(this Vector2 v) => new Vector2(-v.Y, v.X).Normalized();
        public static Vector2 Normal(this Vector2 v) => new(-v.Y, v.X);
        public static Vector2 Normalized(this Vector2 v) => Vector2.Normalize(v);
        public static float DistanceTo(this Vector2 a, Vector2 b) => Vector2.Distance(a, b);
        public static Vector2 ScalarProduct(this Vector2 a, float scalar) => new(a.Y * -scalar, a.X * scalar);
        public static Vector2 VectorProduct(this Vector2 a, Vector2 b) => new(a.Y * b.X, a.X * b.Y);
        public static float Dot(this Vector2 a, Vector2 b) => Vector2.Dot(a, b);

        public static BothAxes Both(this Vector2 v) => new(v);
        public static AnyAxis Any(this Vector2 v) => new(v);

        public static float Max(this Vector2 v) => Math.Max(v.X, v.Y);
        public static float Min(this Vector2 v) => Math.Min(v.X, v.Y);

        public static Vector2 MaxAxis(this Vector2 v) => v.X > v.Y ? new Vector2(v.X, 0) : v.Y > v.X ? new Vector2(0, v.Y) : v;
        public static Vector2 MinAxis(this Vector2 v) => v.X < v.Y ? new Vector2(v.X, 0) : v.Y < v.X ? new Vector2(0, v.Y) : v;

        public static Vector2 MaxSquare(this Vector2 v) => new(v.Max(), v.Max());
        public static Vector2 MinSquare(this Vector2 v) => new(v.Min(), v.Min());

        public static Vector2 Floored(this Vector2 v) => Vector2.Floor(v);
        public static Vector2 Ceiled(this Vector2 v) => Vector2.Ceiling(v);
        public static Vector2 SignCeiled(this Vector2 v) =>
            new(
                /*X*/(v.X > 0 ? v.X.Ceiled() : v.X < 0 ? v.X.Floored() : 0),
                /*Y*/(v.Y > 0 ? v.Y.Ceiled() : v.Y < 0 ? v.Y.Floored() : 0));
        public static Vector2 SignFloored(this Vector2 v) =>
            new(
                /*X*/(v.X > 0 ? v.X.Floored() : v.X < 0 ? v.X.Ceiled() : 0),
                /*Y*/(v.Y > 0 ? v.Y.Floored() : v.Y < 0 ? v.Y.Ceiled() : 0));
        
        public static Vector2 Rounded(this Vector2 v) => Vector2.Round(v);
        public static Vector2 IntCast(this Vector2 v) => new((int)v.X, (int)v.Y);

        public static Vector2 Abs(this Vector2 v) => new(v.AbsX(), v.AbsY());
        public static float AbsX(this Vector2 v) => Math.Abs(v.X);
        public static float AbsY(this Vector2 v) => Math.Abs(v.Y);
    }
}