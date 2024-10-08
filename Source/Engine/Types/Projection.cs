using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Engine.Types
{
    [DebuggerDisplay("{ToString(),nq}")]
    public struct Projection
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public Vector2 Axis { get; set; }

        public Projection(float min, float max, Vector2 axis)
        {
            Min = min;
            Max = max;
            Axis = axis;
        }

        public readonly float GetOverlap(Projection other)
        {
            if (Intersects(other))
                return Math.Min(Max, other.Max) - Math.Max(Min, other.Min);

            return 0;
        }
        public readonly bool Intersects(Projection other) => !(other.Max < Min || other.Min > Max);

        public static float ProjectPoint(Vector2 point, Vector2 axis) => Vector2.Dot(point, axis.Normalized());

        public readonly override string ToString() => $"{Axis}: {Min} ---- {Max}";
    }

}