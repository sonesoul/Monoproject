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

        public static float ProjectPoint(Vector2 point, Vector2 axis) => Vector2.Dot(point, axis.Normalized());

        public readonly override string ToString() => $"{Axis}: {Min} ---- {Max}";
    }

    [DebuggerDisplay("{ToString(),nq}")]
    public struct LineSegment : IProjectable
    {
        public Vector2 Start { get; set; } = Vector2.Zero;
        public Vector2 End { get; set; } = Vector2.Zero;

        public readonly Vector2 Center => (Start + End) / 2;
        public readonly float Distance => Start.DistanceTo(End);
        public readonly Vector2 Direction => End - Start;

        public readonly Vector2 Normal => Direction.UnitNormal();
        public readonly Vector2 UnitNormal => Direction.UnitNormal();
        public readonly Vector2 Perpendicular => Direction.Perpendicular();
        public readonly Vector2 UnitPerpendicular => Direction.Perpendicular();

        public LineSegment(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        public readonly bool IsPointBetween(Vector2 point, float tolerance = 0) => DistanceToPoint(point) <= tolerance;
        public readonly bool IsSegmentBetween(LineSegment other, float tolerance = 0)
        {
            return
                IsPointBetween(other.Start, tolerance) &&
                IsPointBetween(other.End, tolerance);
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

        public readonly bool OLDIsPointBetween(Vector2 point, float tolerance = 0)
        {
            Vector2 start = Start;
            Vector2 end = End;
            Vector2 dir = Direction;

            dir /= dir.Length();

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
        public readonly bool OLDIsSegmentBetween(LineSegment other, float tolerance = 0)
        {
            return
                OLDIsPointBetween(other.Start, tolerance) &&
                OLDIsPointBetween(other.End, tolerance);
        }


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

        public static LineSegment operator +(LineSegment left, LineSegment right) => new(left.Start + right.Start, left.End + right.End);
        public static LineSegment operator +(LineSegment left, Vector2 right) => new(left.Start + right, left.End + right);
        public static LineSegment operator -(LineSegment left, LineSegment right) => new(left.Start - right.Start, left.End - right.End);
        public static LineSegment operator -(LineSegment left, Vector2 right) => new(left.Start - right, left.End - right);
        #endregion
    }

    [DebuggerDisplay("{ToString(),nq}")]
    public struct Polygon : IProjectable
    {
        public readonly Vector2 IntegerPosition => position.IntCast();

        public List<Vector2> Vertices { get; set; }
        public List<LineSegment> Edges { get; private set; } = new();
       
        public readonly List<Vector2> WorldVertices 
        {
            get 
            {
                Vector2 pos = IntegerPosition;
                return Vertices.Select(v => v + pos).ToList();
            }
        }
        public readonly List<LineSegment> WorldEdges 
        { 
            get
            {
                Vector2 pos = IntegerPosition;
                return Edges.Select(e => e + pos).ToList();
            } 
        }

        public Vector2 Center { get; set; } = Vector2.Zero;
        public float Rotation
        {
            readonly get => rotationAngle;
            set
            {
                float oldRotation = rotationAngle;
                rotationAngle = value % 360;

                if (oldRotation == rotationAngle)
                    return;

                if (rotationAngle < 0)
                    rotationAngle += 360;

                UpdateVertices();
            }
        }
        
        public Vector2 position;

        private float rotationAngle = 0;
        private readonly List<Vector2> originalVertices;

        public Polygon(List<Vector2> vertices)
        {
            if (vertices.Count < 3)
                throw new ArgumentOutOfRangeException(nameof(vertices), "The polygon must have at least 3 vertices.");

            position = Vector2.Zero;
            Vertices = vertices;
            originalVertices = new List<Vector2>(vertices);

            Center = DetectCenter();
            UpdateVertices();
        }
       
        private readonly void UpdateVertices()
        {
            Vertices.Clear();

            foreach (var item in originalVertices)
                Vertices.Add(item.RotateAround(Center, rotationAngle));

            Vector2 pos = IntegerPosition;

            Edges.Clear();
            Edges.AddRange(ForEachEdge((p1, p2) => new LineSegment(p1, p2), Vertices));
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
            mtv = GetMTV(other);
            return mtv != Vector2.Zero;
        }
        
        public readonly Vector2 GetMTV(Polygon other)
        {
            float minOverlap = float.MaxValue;
            Vector2 smallestAxis = Vector2.Zero;

            foreach (var axis in GetAxes().Concat(other.GetAxes()))
            {
                Projection proj1 = ProjectOn(axis);
                Projection proj2 = other.ProjectOn(axis);

                if (!proj1.Intersects(proj2))
                    return Vector2.Zero;

                float overlap = proj1.GetOverlap(proj2);
                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    smallestAxis = axis;
                }
            }
            
            Vector2 dir = other.position - position;
            Vector2 mtv = smallestAxis * minOverlap;

            if (Vector2.Dot(dir, mtv) < 0)
                mtv = -mtv;

            return mtv;
        }
        public readonly bool IsPointWithin(Vector2 point)
        {
            bool result = false;
            
            foreach (var item in WorldEdges)
            {
                Vector2 v1 = item.Start;
                Vector2 v2 = item.End;

                LineSegment edge = new(v1, v2);
                if (edge.IsPointBetween(point, 0))
                {
                    return true;
                }

                if ((v1.Y > point.Y) != (v2.Y > point.Y) &&
                    (point.X < (v2.X - v1.X) * (point.Y - v1.Y) / (v2.Y - v1.Y) + v1.X))
                {
                    result = !result;
                }
                
            }
            return result;
        }

        public readonly Vector2 DetectCenter()
        {
            float x = 0, y = 0;

            foreach (var vertex in originalVertices)
            {
                x += vertex.X;
                y += vertex.Y;
            }

            float devidedX = x / Vertices.Count;
            float devidedY = y / Vertices.Count;

            return new Vector2(devidedX, devidedY);
        }

        public readonly LineSegment ClosestEdge(Vector2 point)
        {
            (LineSegment edge, float distance)? closestEdge = null;

            foreach (var edge in WorldEdges)
            {
                float distance = edge.DistanceToPoint(point);

                if (closestEdge == null || distance < closestEdge.Value.distance)
                {
                    closestEdge = (edge,  distance);
                }
            }

            return closestEdge.Value.edge;
        }
        public readonly LineSegment ClosestNormalEdge(Vector2 vector)
        {
            vector.Normalize();

            LineSegment closest = new();

            float maxDot = float.NegativeInfinity;
            
            foreach (var edge in Edges)
            {
                Vector2 edgeNormal = edge.Perpendicular;
               
                float dot = Vector2.Dot(vector, edgeNormal);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    closest = edge;
                }
            }

            return closest;
        }

        public readonly Projection ProjectOn(Vector2 axis)
        {
            var v = Vertices[0];
            float min = Vector2.Dot(axis, v + IntegerPosition);
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

        [Obsolete]
        public readonly List<LineSegment> OLDGetEdges()
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
        public static void ForEachEdge(Action<Vector2, Vector2> action, List<Vector2> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 p1 = vertices[i];
                Vector2 p2 = vertices[(i + 1) % vertices.Count];

                action(p1, p2);
            }
            return;
        }

        public readonly override string ToString() => $"({position.X}, {position.Y}), {rotationAngle}° [{Vertices.Count}]";

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