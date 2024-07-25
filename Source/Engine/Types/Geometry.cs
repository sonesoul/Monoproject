using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Engine.Types
{
    [DebuggerDisplay("{Start} <----> {End} ({Length})")]
    public struct LineSegment
    {
        public Vector2 Start = Vector2.Zero;
        public Vector2 End = Vector2.Zero;
        public Point Center = Point.Zero;

        public readonly float Length => Vector2.Distance(Start, End);
        public readonly Vector2 Direction => End - Start;
        public readonly Vector2 Normal => new Vector2(-Direction.Y, Direction.X).Normalized();

        public LineSegment(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;

            Center = ((Start + End) / 2).ToPoint();
        }

        public readonly bool IsPointOn(Point point, float epsilon = 1e-6f) => IsPointOn(point.ToVector2(), epsilon);
        public readonly bool IsPointOn(Vector2 point, float epsilon = 1e-6f)
        {
            Vector2 segmentDirection = Direction;
            Vector2 pointDirection = point - Start;

            float crossProduct = segmentDirection.X * pointDirection.Y - segmentDirection.Y * pointDirection.X;
            if (Math.Abs(crossProduct) > epsilon)
                return false;

            float dotProduct = Vector2.Dot(pointDirection, segmentDirection);
            if (dotProduct < 0 || dotProduct > segmentDirection.LengthSquared())
                return false;

            return true;
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
        public readonly bool Intersects(Projection other) => !(other.Max < Min || other.Min > Max);
    }
    public struct Polygon
    {
        public Vector2 position;
        private float _rotationAngle = 0;
        private readonly List<Vector2> _originalVertices;
        public Vector2 _center = Vector2.Zero;
        
        public readonly Vector2 FlooredPosition => new((int)Math.Floor(position.X), (int)Math.Floor(position.Y));
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
            _center = AutoCenter();
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

            mtv = smallestAxis * minOverlap;
            return true;
        }

        public readonly Vector2 AutoCenter()
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
            float min = Vector2.Dot(axis, Vertices[0] + FlooredPosition);
            float max = min;

            foreach (var vertex in Vertices)
            {
                float projection = Vector2.Dot(axis, vertex + FlooredPosition);
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
                axes.Add(new(p1 + FlooredPosition, p2 + FlooredPosition));
            }
            return axes;
        }
        public static Vector2 RotatePoint(Vector2 point, Vector2 origin, float rotation)
        {
            rotation = rotation.AsRadians();

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
                    new(-width, -height), //top left
                    new(width, -height), //top right
                    new(width, height), //bottom right
                    new(-width, height) //bottom left
                };
        }
        public static List<Vector2> RightTriangleVerts(float width, float height)
        {
            List<Vector2> rectVerts = RectangleVerts(width, height);
            rectVerts.RemoveAt(1);
            return rectVerts;
        }

        public static Polygon Rectangle(float width, float height) => new(RectangleVerts(width, height));
        public static Polygon RightTriangle(float width, float height) => new(RightTriangleVerts(width, height));
    }
}