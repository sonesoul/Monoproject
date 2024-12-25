using Engine.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Engine.Types
{
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
                Vertices.Add(item.RotateAround(Center, rotationAngle).Rounded());

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
        public readonly bool ContainsPoint(Vector2 point)
        {
            bool result = false;

            foreach (var item in WorldEdges)
            {
                Vector2 v1 = item.Start;
                Vector2 v2 = item.End;

                LineSegment edge = new(v1, v2);
                if (edge.ContainsPoint(point, 0))
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
                    closestEdge = (edge, distance);
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
        public static Polygon Rectangle(Vector2 size) => new(RectangleVerts(size.X, size.Y));
        public static Polygon RightTriangle(float width, float height) => new(RightTriangleVerts(width, height));
        #endregion
    }
}