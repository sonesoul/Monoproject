using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Engine.Types
{
    public struct LineSegment
    {
        public Vector2 Start = Vector2.Zero;
        public Vector2 End = Vector2.Zero;
        public Point Center = Point.Zero;

        public readonly float Distance => Vector2.Distance(Start, End);
        public readonly Vector2 Direction => End - Start;
        public readonly Vector2 Normal => new(-Direction.X, -Direction.Y);

        public LineSegment(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;

            Center = ((Start + End) / 2).ToPoint();
        }
        public readonly bool IsPointOn(Point point, float epsilon = 1e-6f)
        {
            float crossProduct =
                (point.X - Start.X) * (End.Y - Start.Y) -
                (point.Y - Start.Y) * (End.X - Start.X);

            if (Math.Abs(crossProduct) > epsilon)
                return false;

            float dotProduct =
                (point.X - Start.X) * (End.X - Start.X) +
                (point.Y - Start.Y) * (End.Y - Start.Y);

            if (dotProduct < 0)
                return false;

            float squaredLengthBA =
                    (End.X - Start.X) * (End.X - Start.X) +
                    (End.Y - Start.Y) * (End.Y - Start.Y);

            if (dotProduct > squaredLengthBA)
                return false;

            return true;
        }

        public readonly bool IntersectsWithRay(Vector2 rayOrigin, Vector2 rayDirection, out Vector2 intersectionPoint)
        {
            intersectionPoint = Vector2.Zero;

            float dx1 = Direction.X;
            float dy1 = Direction.Y;
            float dx2 = rayDirection.X;
            float dy2 = rayDirection.Y;

            float denominator = dy2 * dx1 - dx2 * dy1;

            if (denominator == 0)
                return false;

            float ua = (dx2 * (Start.Y - rayOrigin.Y) - dy2 * (Start.X - rayOrigin.X)) / denominator;
            float ub = (dx1 * (Start.Y - rayOrigin.Y) - dy1 * (Start.X - rayOrigin.X)) / denominator;

            if (ua >= 0 && ua <= 1 && ub >= 0)
            {
                intersectionPoint = new Vector2(Start.X + ua * dx1, Start.Y + ua * dy1);
                return true;
            }

            return false;
        }
        public readonly bool IntersectsWith(LineSegment other, out Vector2 intersectionPoint)
        {
            intersectionPoint = Vector2.Zero;

            float dx1 = Direction.X;
            float dy1 = Direction.Y;

            float dx2 = other.Direction.X;
            float dy2 = other.Direction.Y;

            float denominator = dy2 * dx1 - dx2 * dy1;

            if (denominator == 0)
                return false;

            float ua = (dx2 * (Start.Y - other.Start.Y) - dy2 * (Start.X - other.Start.X)) / denominator;
            float ub = (dx1 * (Start.Y - other.Start.Y) - dy1 * (Start.X - other.Start.X)) / denominator;

            if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
            {
                intersectionPoint = new(Start.X + ua * dx1, Start.Y + ua * dy1);
                return true;
            }

            return false;
        }
        public readonly bool IsCollinearWith(LineSegment other, float epsilon = 1e-6f)
        {
            Vector2 dir1 = Direction;
            Vector2 dir2 = other.Direction;

            float value = Math.Abs(dir1.X * dir2.Y - dir1.Y * dir2.X);
            bool result = value < epsilon;
            return result;
        }
        public readonly bool OverlapsWith(LineSegment other, float epsilon = 1e-6f)
        {
            if (!IsCollinearWith(other, epsilon))
                return false;

            Vector2 dir = Direction;

            float start1 = Vector2.Dot(Start, dir);
            float end1 = Vector2.Dot(End, dir);
            float start2 = Vector2.Dot(other.Start, dir);
            float end2 = Vector2.Dot(other.End, dir);

            if (start1 > end1)
                (start1, end1) = (end1, start1);

            if (start2 > end2)
                (start2, end2) = (end2, start2);

            return (start1 <= end2 + epsilon) && (start2 <= end1 + epsilon);
        }
        public readonly LineSegment? GetOverlappingSegment(LineSegment other)
        {
            if (IsCollinearWith(other) && OverlapsWith(other))
            {
                float minX = Math.Max(Math.Min(Start.X, End.X), Math.Min(other.Start.X, other.End.X));
                float maxX = Math.Min(Math.Max(Start.X, End.X), Math.Max(other.Start.X, other.End.X));

                Vector2 newStart = Start.X < End.X ? Start : End;
                Vector2 newEnd = other.Start.X < other.End.X ? other.Start : other.End;

                newStart = (newStart.X < newEnd.X) ? newEnd : newStart;
                newEnd = (newStart.X > newEnd.X) ? newStart : newEnd;

                return new LineSegment(new Vector2(minX, newStart.Y), new Vector2(maxX, newEnd.Y));
            }
            return null;
        }
    }
    public struct Projection
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public Projection(float min, float max)
        {
            Min = min;
            Max = max;
        }
        public readonly float GetOverlap(Projection other)
        {
            if (Intersects(other))
            {
                return Math.Min(Max, other.Max) - Math.Max(Min, other.Min);
            }
            return 0;
        }
        public readonly bool Intersects(Projection other) => !(other.Max < Min || other.Min > Max); //(Max >= other.Min && other.Max >= Min);
    }
    public struct Polygon
    {
        public Vector2 position;
        private float _rotationAngle = 0;
        private readonly List<Vector2> _originalVertices;
        private readonly Vector2 _center = Vector2.Zero;

        public readonly Vector2 Center => _center;

        public readonly Vector2 IntegerPosition => new((int)Math.Floor(position.X), (int)Math.Floor(position.Y));
        public List<Vector2> Vertices { get; private set; }
        public float Rotation
        {
            readonly get => _rotationAngle;
            set
            {
                float oldRotation = _rotationAngle;
                _rotationAngle = value % 360;

                if (oldRotation == _rotationAngle)
                    return;

                if (_rotationAngle < 0)
                    _rotationAngle += 360;

                UpdateVertices();
            }
        }

        public Polygon(Vector2 position, List<Vector2> vertices)
        {
            this.position = position;
            Vertices = vertices;
            _originalVertices = new List<Vector2>(vertices);
            _center = GetCenter();
        }
        public Polygon(List<Vector2> vertices) : this(Vector2.Zero, vertices) { }
        private readonly void UpdateVertices()
        {
            Vertices.Clear();

            foreach (var item in _originalVertices)
                Vertices.Add(RotatePoint(item, _center, _rotationAngle));
        }

        public readonly bool IntersectsWith(Polygon other)
        {
            foreach (var axis in GetAxes().Concat(other.GetAxes()))
            {
                Projection proj1 = GetProjection(axis);
                Projection proj2 = other.GetProjection(axis);

                if (!proj1.Intersects(proj2))
                    return false;
            }

            return true;
        }
        public readonly bool IntersectsWith(Polygon other, out Vector2 mtv)
        {
            mtv = Vector2.Zero;
            float minOverlap = float.MaxValue;
            Vector2 smallestAxis = Vector2.Zero;

            foreach (var axis in GetAxes().Concat(other.GetAxes()))
            {
                Projection proj1 = GetProjection(axis);
                Projection proj2 = other.GetProjection(axis);

                if (!proj1.Intersects(proj2))
                    return false;

                float overlap = proj1.GetOverlap(proj2);
                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    smallestAxis = axis;
                }
            }

            mtv = smallestAxis * minOverlap * 1f;
            return true;
        }

        public readonly Vector2 GetCenter()
        {
            float x = 0, y = 0;

            foreach (var vertex in _originalVertices)
            {
                x += vertex.X;
                y += vertex.Y;
            }

            float devidedX = x / Vertices.Count;
            float devidedY = y / Vertices.Count;

            return new Vector2(devidedX, devidedY);
        }
        public readonly Projection GetProjection(Vector2 axis)
        {
            float min = Vector2.Dot(axis, Vertices[0] + IntegerPosition);
            float max = min;

            foreach (var vertex in Vertices)
            {
                float projection = Vector2.Dot(axis, vertex + IntegerPosition);
                if (projection < min)
                    min = projection;
                if (projection > max)
                    max = projection;
            }

            return new(min, max);
        }
        public readonly List<Vector2> GetAxes()
        {
            List<Vector2> axes = new();
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vector2 p1 = Vertices[i];
                Vector2 p2 = Vertices[(i + 1) % Vertices.Count];
                Vector2 edge = p2 - p1;

                axes.Add(Vector2.Normalize(new Vector2(-edge.Y, edge.X)));
            }
            return axes;
        }
        public readonly List<LineSegment> GetEdges()
        {
            List<LineSegment> axes = new();
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vector2 p1 = Vertices[i];
                Vector2 p2 = Vertices[(i + 1) % Vertices.Count];
                Vector2 edge = p2 - p1;
                axes.Add(new(p1 + IntegerPosition, p2 + IntegerPosition));
            }
            return axes;
        }
        public static Vector2 RotatePoint(Vector2 point, Vector2 origin, float rotation)
        {
            rotation = rotation.ToRad();

            float cos = (float)Math.Cos(rotation);
            float sin = (float)Math.Sin(rotation);

            Vector2 translatedPoint = point - origin;

            float newX = translatedPoint.X * cos - translatedPoint.Y * sin;
            float newY = translatedPoint.X * sin + translatedPoint.Y * cos;

            return new Vector2(newX, newY) + origin;
        }


        public static List<Vector2> RectangleVerts(float width, float height)
        {
            width /= 2;
            height /= 2;

            return new()
                {
                    new(-width, -height),
                    new(width, -height),
                    new(width, height),
                    new(-width, height)
                };
        }
        public static List<Vector2> RightTriangleVerts(float width, float height)
        {
            return new()
                {
                    new((int)(-width / 3), (int)height / 3),
                    new((int)width, (int)height / 3),
                    new((int)-width / 3, (int)-height),
                };
        }

        public static Polygon Rectangle(float width, float height) => new(RectangleVerts(width, height));
        public static Polygon RightTriangle(float width, float height) => new(RightTriangleVerts(width, height));
    }
}