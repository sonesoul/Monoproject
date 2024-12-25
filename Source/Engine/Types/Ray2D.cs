using System.Diagnostics;

namespace Engine.Types
{
    [DebuggerDisplay("{ToString(),nq}")]
    public struct Ray2D
    {
        public Vector2 StartPoint { get; set; }
        public Vector2 Direction { get; set; }

        public Ray2D(Vector2 start, Vector2 dir)
        {
            StartPoint = start;
            Direction = dir.Normalized();
        }

        public readonly Vector2 GetPoint(float distance) => StartPoint + Direction * distance;
        public readonly bool Intersects(Ray2D other, out Vector2 intersection)
        {
            intersection = default;

            Vector2 r = StartPoint;
            Vector2 d = Direction;
            Vector2 p = other.StartPoint;
            Vector2 q = other.Direction;

            float denominator = d.X * q.Y - d.Y * q.X;

            if (denominator.Abs() < float.Epsilon)
                return false;

            float t = ((p.X - r.X) * q.Y - (p.Y - r.Y) * q.X) / denominator;
            float s = ((p.X - r.X) * d.Y - (p.Y - r.Y) * d.X) / denominator;

            if (t >= 0 && s >= 0)
            {
                intersection = r + t * d;
                return true;
            }

            return false;
        }
        public readonly bool Intersects(LineSegment segment, out Vector2 intersection)
        {
            intersection = default;

            Vector2 r = StartPoint;
            Vector2 d = Direction;
            Vector2 p = segment.Start;
            Vector2 q = segment.End - segment.Start;

            float denominator = q.X * d.Y - q.Y * d.X;
            if (denominator.Abs() < float.Epsilon)
                return false;

            float t = ((p.X - r.X) * d.Y - (p.Y - r.Y) * d.X) / denominator;
            float u = ((p.X - r.X) * q.Y - (p.Y - r.Y) * q.X) / denominator;

            if (t >= 0 && u >= 0 && u <= 1)
            {
                intersection = r + t * d;
                return true;
            }

            return false;
        }
        public readonly bool IsPointOn(Vector2 point, out float distance)
        {
            Vector2 toPoint = point - StartPoint;

            float dotProduct = Vector2.Dot(toPoint, Direction);
            if (dotProduct < 0)
            {
                distance = 0;
                return false;
            }

            Vector2 projectedPoint = StartPoint + dotProduct * Direction;
            if (Vector2.DistanceSquared(point, projectedPoint) < float.Epsilon)
            {
                distance = dotProduct;
                return true;
            }

            distance = 0;
            return false;
        }

        public readonly override string ToString() => $"{StartPoint}-->{Direction}";
    }
}