using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Engine.Types
{
    public interface IProjectable
    {
        public Projection ProjectOn(Vector2 axis);
    }

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

        public static float ProjectPoint(Vector2 point, Vector2 axis) => Vector2.Dot(point, axis);

        public readonly override string ToString() => $"{Axis}: {Min} ---- {Max}";
    }
    
    [DebuggerDisplay("{ToString(),nq}")] 
    public struct LineSegment : IProjectable
    {
        public Vector2 Start { get; set; } = Vector2.Zero;
        public Vector2 End { get; set; } = Vector2.Zero;

        public readonly Vector2 Center => (Start + End) / 2;
        public readonly float Distance => Vector2.Distance(Start, End);
        public readonly Vector2 Direction => End - Start;
        public readonly Vector2 Normal => Direction.UnitNormal();
        
        public LineSegment(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        public readonly bool IsPointBetween(Vector2 point, float tolerance)
        {
            Vector2 start = Start;
            Vector2 end = End;

            Vector2 dir = Direction;
            float length = dir.Length();
            dir /= length;

            Vector2 perpendicular = dir.Perpendicular();

            Vector2 offset = (perpendicular * tolerance).Abs();

            Vector2 rectStart = start - offset - (dir * tolerance);
            Vector2 rectEnd = end + offset + (dir * tolerance);
            
            Vector2 min = new(
                Math.Min(rectStart.X, rectEnd.X),
                Math.Min(rectStart.Y, rectEnd.Y));
            Vector2 max = new(
                Math.Max(rectStart.X, rectEnd.X),            
                Math.Max(rectStart.Y, rectEnd.Y));

            bool betweenX = point.X >= min.X && point.X <= max.X;
            bool betweenY = point.Y >= min.Y && point.Y <= max.Y;

            return betweenX && betweenY;
        }
        public readonly bool IsSegmentBetween(LineSegment other, float tolerance) => IsPointBetween(other.Start, tolerance) && IsPointBetween(other.End, tolerance);
        
        public readonly bool HasCommonVertex(LineSegment other) => Start == other.Start || End == other.End || Start == other.End || End == other.Start;
        public readonly float DistanceToPoint(Vector2 point)
        {
            Vector2 lineDir = Direction;
            Vector2 pointDir = point - Start;

            float lineLength = lineDir.Length();
            lineDir /= lineLength;

            float projection = Vector2.Dot(pointDir, lineDir);
            projection = Math.Clamp(projection, 0, lineLength);

            Vector2 closestPoint = Start + lineDir * projection;
            return Vector2.Distance(point, closestPoint);
        }
        public readonly void Deconstruct(out Vector2 start, out Vector2 end)
        {
            start = Start;
            end = End;
        }

        public readonly Projection ProjectOn(Vector2 axis)
        {
            float start = Vector2.Dot(Start, axis);
            float end = Vector2.Dot(End, axis);
            return new(Math.Min(start, end), Math.Max(start, end), axis);
        }
        public readonly LineSegment Rounded() => new(Start.Rounded(), End.Rounded());

        #region ObjectOverrides
        private static bool Equals(LineSegment left, LineSegment right) =>
            (left.Start == right.Start && left.End == right.End) ||
            (left.Start == right.End && left.End == right.Start);

        public readonly override bool Equals(object obj)
        {
            if(obj is LineSegment other)
                return Equals(this, other);
            else 
                return false;
        }
        public readonly override int GetHashCode() => HashCode.Combine(Start, End);
        public readonly override string ToString() => $"{Start} ---- {End} ({Distance})";

        public static bool operator ==(LineSegment left, LineSegment right) => Equals(left, right);
        public static bool operator !=(LineSegment left, LineSegment right) => !(left == right);
        #endregion
    }

    [DebuggerDisplay("{ToString(),nq}")]
    public struct Polygon : IProjectable
    {
        public Vector2 position;
        private float _rotationAngle = 0;
        private readonly List<Vector2> _originalVertices;

        public readonly Vector2 IntegerPosition => position.Rounded();
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
        public Vector2 Center { get; set; } = Vector2.Zero;
        public List<Vector2> Vertices { get; private set; }

        public Polygon(Vector2 position, List<Vector2> vertices)
        {
            this.position = position;
            Vertices = vertices;
            _originalVertices = new List<Vector2>(vertices);
            Center = DetectCenter();
        }
        public Polygon(List<Vector2> vertices) : this(Vector2.Zero, vertices) { }

        private readonly void UpdateVertices()
        {
            Vertices.Clear();

            foreach (var item in _originalVertices)
                Vertices.Add(RotatePoint(item, Center, _rotationAngle));
        }

        public readonly bool IntersectsWith(Polygon other)
        {
            foreach (var axis in GetAxes().Concat(other.GetAxes()))
            {
                Projection proj1 = ProjectOn(axis);
                Projection proj2 = other.ProjectOn(axis);

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
                Projection proj1 = ProjectOn(axis);
                Projection proj2 = other.ProjectOn(axis);

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

        public readonly Vector2 DetectCenter()
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

        public readonly Projection ProjectOn(Vector2 axis)
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

            return new(min, max, axis);
        }
        public readonly List<Vector2> GetAxes() => ForEachEdge((p1, p2) =>
        {
            Vector2 edge = p2 - p1;
            return new Vector2(-edge.Y, edge.X).Normalized();
        }, Vertices);
        public readonly List<LineSegment> GetEdges()
        {
            Vector2 pos = IntegerPosition; 
            return ForEachEdge((p1, p2) => new LineSegment(p1 + pos, p2 + pos), Vertices);
        }
        
        public static List<T> ForEachEdge<T>(Func<Vector2, Vector2, T> action, List<Vector2> vertices)
        {
            List<T> edges = new();
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 p1 = vertices[i];
                Vector2 p2 = vertices[(i + 1) % vertices.Count];

                edges.Add(action(p1, p2));
            }
            return edges;
        }
        public static List<T> ForEachVertex<T>(Func<Vector2, T> action, List<Vector2> vertices)
        {
            List<T> edges = new();
            for (int i = 0; i < vertices.Count; i++)
                edges.Add(action(vertices[i]));
            
            return edges;
        }
        public static Vector2 RotatePoint(Vector2 point, Vector2 origin, float rotation)
        {
            rotation = rotation.AsRad();

            float cos = (float)Math.Cos(rotation);
            float sin = (float)Math.Sin(rotation);

            Vector2 translatedPoint = point - origin;

            float newX = translatedPoint.X * cos - translatedPoint.Y * sin;
            float newY = translatedPoint.X * sin + translatedPoint.Y * cos;

            return new Vector2(newX, newY) + origin;
        }

        public readonly override string ToString() => $"({position.X}, {position.Y}), {_rotationAngle}° [{Vertices.Count}]";

        #region ShapeSamples
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
        #endregion
    }

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

    public readonly struct Grid<T>
    {
        public int Rows { get; init; }
        public int Columns { get; init; }

        private readonly T[,] cells;

        public Grid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            cells = new T[rows, columns];
        }
        public Grid(int rowsCols)
        {
            Rows = rowsCols;
            Columns = rowsCols;
            cells = new T[rowsCols, rowsCols];
        }
        public readonly T GetCell(int row, int column) => cells[row, column];

        public readonly void ForEach(Action<T> action)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    action(cells[j, i]);
                }
            }
        }

        public readonly void SetCell(T value, int row, int column) => cells[row, column] = value;
    }
}