using Engine.Types.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Engine.Types
{
    [DebuggerDisplay("{ToString(),nq}")]
    public struct LineSegment : IProjectable
    {
        public Vector2 Start { get; set; } = Vector2.Zero;
        public Vector2 End { get; set; } = Vector2.Zero;

        public readonly Vector2 Center => (Start + End) / 2;
        public readonly float Distance => Start.DistanceTo(End);
        public readonly Vector2 Direction => End - Start;

        public readonly Vector2 UnitNormal => Direction.UnitNormal();
        public readonly Vector2 Perpendicular => Direction.Perpendicular();

        public LineSegment(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        public readonly bool ContainsPoint(Vector2 point, float tolerance = 0) => DistanceToPoint(point) <= tolerance;
        public readonly bool IsSegmentBetween(LineSegment other, float tolerance = 0)
        {
            return
                ContainsPoint(other.Start, tolerance) &&
                ContainsPoint(other.End, tolerance);
        }

        public readonly bool HasCommonVertex(LineSegment other, out Vector2 vertex)
        {
            vertex = Vector2.Zero;

            if (Start == other.Start || Start == other.End)
            {
                vertex = Start;
                return true;
            }
            else if (End == other.Start || End == other.End)
            {
                vertex = End;
                return true;
            }

            return false;
        }

        public readonly Vector2 ClosestPoint(Vector2 point)
        {
            Vector2 ab = Direction;
            Vector2 ap = point - Start;

            float proj = Vector2.Dot(ap, ab);
            float d = (proj / ab.LengthSquared()).Clamp(0, 1);

            return Start + ab * d;
        }
        public readonly float DistanceToPoint(Vector2 point) => Vector2.Distance(point, ClosestPoint(point));

        public readonly bool Intersects(LineSegment other, out Vector2 point)
        {
            point = Vector2.Zero;

            float denominator = Denominator(other);

            if (denominator == 0)
            {
                return false;
            }

            float ua = ((other.End.X - other.Start.X) * (Start.Y - other.Start.Y) -
                         (other.End.Y - other.Start.Y) * (Start.X - other.Start.X)) / denominator;

            float ub = ((End.X - Start.X) * (Start.Y - other.Start.Y) -
                         (End.Y - Start.Y) * (Start.X - other.Start.X)) / denominator;

            if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
            {
                float intersectionX = Start.X + ua * (End.X - Start.X);
                float intersectionY = Start.Y + ua * (End.Y - Start.Y);

                point = new Vector2(intersectionX, intersectionY);
                return true;
            }

            return false;
        }
        public readonly bool Intersects(LineSegment other) => Intersects(other, out _);

        public readonly void Deconstruct(out Vector2 start, out Vector2 end)
        {
            start = Start;
            end = End;
        }

        public readonly float Denominator(LineSegment other)
        {
            Vector2 dir1 = Direction;
            Vector2 dir2 = other.Direction;

            return dir1.Cross(dir2);
        }

        public readonly Projection ProjectOn(Vector2 axis)
        {
            float start = Projection.ProjectPoint(Start, axis);
            float end = Projection.ProjectPoint(End, axis);
            return new(Math.Min(start, end), Math.Max(start, end), axis);
        }
        public readonly LineSegment Rounded() => new(Start.Rounded(), End.Rounded());

        #region ObjectOverrides
        private static bool Equals(LineSegment left, LineSegment right) =>
            (left.Start == right.Start && left.End == right.End) ||
            (left.Start == right.End && left.End == right.Start);

        public readonly override bool Equals(object obj)
        {
            if (obj is LineSegment other)
                return Equals(this, other);
            else
                return false;
        }
        public readonly override int GetHashCode() => HashCode.Combine(Start, End);
        public readonly override string ToString() => $"{Start} ---- {End} ({Distance})";

        public static bool operator ==(LineSegment left, LineSegment right) => Equals(left, right);
        public static bool operator !=(LineSegment left, LineSegment right) => !(left == right);

        public static LineSegment operator +(LineSegment left, LineSegment right) => new(left.Start + right.Start, left.End + right.End);
        public static LineSegment operator +(LineSegment left, Vector2 right) => new(left.Start + right, left.End + right);
        public static LineSegment operator -(LineSegment left, LineSegment right) => new(left.Start - right.Start, left.End - right.End);
        public static LineSegment operator -(LineSegment left, Vector2 right) => new(left.Start - right, left.End - right);
        #endregion
    }


}
