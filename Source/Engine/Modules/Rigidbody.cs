using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Types;
using Monoproject;
using System.ComponentModel;
using System.Windows.Forms.VisualStyles;

namespace Engine.Modules
{
    public class Rigidbody : ObjectModule
    {
        #region Fields
        public float Mass { get; set; } = 1;
        public float GravityScale { get; set; } = 1;
        public float Bounciness { get; set; } = 0.5f;
        public float Friction { get; set; } = 0.3f;
        public float Windage { get; set; } = 0.3f;

        public float Angle { get; set; } = 0;
        public float Torque { get; set; } = 0;
        public float AngularVelocity { get; set; } = 0;
        public float AngularAcceleration { get; set; } = 0;
        public float MomentOfInertia { get; set; } = 1;

        public Collider CollModule { get; private set; }

        public Vector2 forces = Vector2.Zero;
        public Vector2 velocity = Vector2.Zero;

        public const float Gravity = 9.81f;
        public const float FixedDelta = 1.0f / 240.0f;
        public const float ZeroThreshold = 0.005f;
        public const float Tolerance = 1;
        private float updateTimeBuffer = 0.0f;
        #endregion

        private struct EdgeTouch
        {
            public Vector2 vertex;
            public LineSegment edge;
            public EdgeTouch(Vector2 vertex, LineSegment edge)
            {
                this.vertex = vertex;
                this.edge = edge;
            }

            public static List<EdgeTouch> FromSingleCorner(List<CornerTouch> touches1, List<CornerTouch> touches2)
            {
                List<EdgeTouch> result = new();

                var touchPairs = from t1 in touches1
                                 from t2 in touches2
                                 select new { t1, t2 };

                foreach (var pair in touchPairs)
                {
                    var edgeCombinations = from e1 in new[] { pair.t1.edge1, pair.t1.edge2 }
                                           from e2 in new[] { pair.t2.edge1, pair.t2.edge2 }
                                           select new { e1, e2 };

                    foreach (var comb in edgeCombinations)
                    {
                        var e1 = comb.e1;
                        var e2 = comb.e2;

                        if (e1.IsSegmentBetween(e2, Tolerance))
                        {
                            e1.Deconstruct(out var a, out var b);
                            e2.Deconstruct(out var c, out var d);

                            if (c.DistanceTo(a) > Tolerance && c.DistanceTo(b) > Tolerance)
                                result.Add(new(d, e2));
                            else if (d.DistanceTo(a) > Tolerance && d.DistanceTo(b) > Tolerance)
                                result.Add(new(c, e2));
                        }
                    }
                }

                return result;
            }
            public static void Refine(List<EdgeTouch> edgeTouches, List<LineSegment> edges)
            {
                HashSet<EdgeTouch> toAdd = new();

                for (int i = 0; i < edgeTouches.Count; i++)
                {
                    for (int j = 0; j < edgeTouches.Count; j++)
                    {
                        EdgeTouch e1 = edgeTouches[i];
                        EdgeTouch e2 = edgeTouches[j];

                        if (e1.edge == e2.edge)
                            continue;

                        if (TryGetCommonVertex(e1.edge, e2.edge, out var commonEnd))
                        {
                            foreach (var edge in edges)
                            {
                                if (edge.IsPointBetween(commonEnd, Tolerance))
                                {
                                    toAdd.Add(new(commonEnd, edge));

                                    edgeTouches.Remove(e1);
                                    edgeTouches.Remove(e2);
                                }
                            }
                        }
                    }
                }

                edgeTouches.AddRange(toAdd);
            }
            public static void Adjust(List<EdgeTouch> edgeTouches1, List<EdgeTouch> edgeTouches2, Polygon polygon1, Polygon polygon2)
            {
                List<LineSegment> edges1 = polygon1.GetEdges();
                List<LineSegment> edges2 = polygon2.GetEdges();

                EdgeTouch? vertexTouch1 = null;
                EdgeTouch? vertexTouch2 = null;

                foreach (var et1 in edgeTouches1)
                {
                    foreach (var et2 in edgeTouches2)
                    {
                        var e = et2.edge;

                        if(et1.vertex == e.Start.Rounded() || et1.vertex == e.End.Rounded())
                            vertexTouch1 = new(et1.vertex, et1.edge);
                    }
                }
                foreach (var et1 in edgeTouches2)
                {
                    foreach (var et2 in edgeTouches1)
                    {
                        var e = et2.edge;

                        if (et1.vertex == e.Start.Rounded() || et1.vertex == e.End.Rounded())
                            vertexTouch2 = new(et1.vertex, et1.edge);
                    }
                }

                if(vertexTouch1 != null && vertexTouch2 != null)
                {
                    var vt1 = vertexTouch1.Value;
                    var vt2 = vertexTouch2.Value;
                    foreach (var e1 in edges1)
                    {
                        if (e1.Rounded() == vt1.edge)
                        {
                            vt1.edge = e1;

                            foreach (var e2 in edges2)
                            {
                                if (vt1.vertex == e2.Start.Rounded())
                                    vt1.vertex = e2.Start;
                                else if (vt1.vertex == e2.End.Rounded())
                                    vt1.vertex = e2.End;
                                break;
                            }

                            break;
                        }
                    }
                    foreach (var e2 in edges2)
                    {
                        if(e2.Rounded() == vt2.edge)
                        {
                            vt2.edge = e2;

                            foreach (var e1 in edges1)
                            {
                                if (vt2.vertex == e1.Start.Rounded())
                                    vt2.vertex = e1.Start;
                                else if (vt2.vertex == e1.End.Rounded())
                                    vt2.vertex = e1.End;
                                break;
                            }

                            break;
                        }
                    }

                    var distance1 = vt1.edge.DistanceToPoint(vt1.vertex);
                    var distance2 = vt2.edge.DistanceToPoint(vt2.vertex);

                    Console.WriteLine(distance1);
                    Console.WriteLine(distance2);

                    if (distance1 > distance2)
                        edgeTouches1.Remove(vertexTouch1.Value);
                    else if (distance2 > distance1)
                        edgeTouches2.Remove(vertexTouch2.Value);
                }
            }
            public static bool TryGetCommonVertex(LineSegment segment1, LineSegment segment2, out Vector2 commonEnd)
            {
                commonEnd = Vector2.Zero;
                Vector2[] ends1 = { segment1.Start, segment1.End };
                Vector2[] ends2 = { segment2.Start, segment2.End };

                foreach (var end1 in ends1)
                {
                    foreach (var end2 in ends2)
                    {
                        if (end1 == end2)
                        {
                            commonEnd = end1;
                            return true;
                        }
                    }
                }

                return false;
            }
        }
        private struct CornerTouch
        {
            public Vector2 commonVertex;
            public LineSegment edge1;
            public LineSegment edge2;
            public CornerTouch(Vector2 position, LineSegment edge1, LineSegment edge2)
            {
                this.commonVertex = position;
                this.edge1 = edge1;
                this.edge2 = edge2;
            }

            public static List<CornerTouch> ExtractFrom(List<EdgeTouch> items)
            {
                List<CornerTouch> resultPairs = new();
                var groups = items.GroupBy(item => item.vertex);

                HashSet<EdgeTouch> toRemove = new();

                foreach (var group in groups)
                {
                    var edges = group.ToList();

                    if (edges.Select(item => item.edge).Distinct().ToList().Count <= 1)
                        continue;

                    for (int i = 0; i < edges.Count; i++)
                    {
                        for (int j = i + 1; j < edges.Count; j++)
                        {
                            var e1 = edges[i].edge;
                            var e2 = edges[j].edge;

                            if (e1 != e2)
                            {
                                resultPairs.Add(new(edges[i].vertex, e1, e2));

                                toRemove.Add(edges[i]);
                                toRemove.Add(edges[j]);
                            }
                        }
                    }
                }

                items.RemoveAll(i => toRemove.Contains(i));

                return resultPairs;
            }
            public static List<EdgeTouch> FindCommonEdges(List<CornerTouch> corners)
            {
                List<EdgeTouch> result = new();
                for (int i = 0; i < corners.Count; i++)
                {
                    var c1 = corners[i];
                    for (int j = i + 1; j < corners.Count; j++)
                    {
                        var c2 = corners[j];

                        if (c1.commonVertex == c2.commonVertex)
                            continue;

                        LineSegment fe1 = c1.edge1;
                        LineSegment fe2 = c1.edge2;

                        LineSegment se1 = c2.edge1;
                        LineSegment se2 = c2.edge2;

                        LineSegment? commonEdge = null;

                        if (fe1 == se1 || fe1 == se2)
                            commonEdge = fe1;
                        else if (se1 == fe1 || se1 == fe2)
                            commonEdge = se1;


                        if (commonEdge != null)
                        {
                            result.Add(new(c1.commonVertex, commonEdge.Value));
                            result.Add(new(c2.commonVertex, commonEdge.Value));
                        }

                    }
                }
                return result;
            }
        }

        public Rigidbody(ModularObject owner) : base(owner)
        {
            if (owner.TryGetModule<Collider>(out var module))
                CollModule = module;
            else
                CollModule = owner.AddModule<Collider>();

            CollModule.OnDispose += OnColliderDestruct;
            CollModule.OnCheckFinish += Update;
        }

        public void AddForce(Vector2 force) => forces += force;
        public void SetCollider(Collider newCollider)
        {
            CollModule = newCollider;
            Owner.ReplaceModule(CollModule);
            CollModule.OnDispose += OnColliderDestruct;
            CollModule.OnCheckFinish += Update;
        }

        private void Update(GameTime gameTime)
        {
            updateTimeBuffer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            while (updateTimeBuffer >= FixedDelta)
            {
                velocity += forces / Mass * FixedDelta;
                forces = Vector2.Zero;

                AngularVelocity += Torque * FixedDelta;
                Torque = 0;

                foreach (var item in CollModule.Intersections)
                {
                    if (item.Owner.TryGetModule(out Rigidbody otherRb))
                        HandlePhysical(this, otherRb);
                    else
                        HandleOthers(this, item);
                }

                //ApplyGravity();
                ApplyFriction(Windage);
                AngularVelocity *= 0.9955f;
                
                CollModule.info = velocity.ToString();

                Angle = AngularVelocity;
                Owner.position += velocity;
                Owner.Rotation += Angle;

                updateTimeBuffer -= FixedDelta;
            }
        }
        public static void HandlePhysical(Rigidbody first, Rigidbody second)
        {
            List<EdgeTouch> touches = GetTouches(second.CollModule, first.CollModule);
            List<EdgeTouch> otherTouches = GetTouches(first.CollModule, second.CollModule);


            List<CornerTouch> cornerTouches = CornerTouch.ExtractFrom(touches);

            if (cornerTouches.Count > 1)
                touches = touches.Concat(CornerTouch.FindCommonEdges(cornerTouches)).ToList();
            else if(cornerTouches.Count == 1)
                touches = touches.Concat(EdgeTouch.FromSingleCorner(cornerTouches, CornerTouch.ExtractFrom(GetTouches(first.CollModule, second.CollModule)))).ToList();

            EdgeTouch.Refine(touches, second.CollModule.polygon.GetEdges());
            EdgeTouch.Refine(otherTouches, first.CollModule.polygon.GetEdges());
            EdgeTouch.Adjust(touches, otherTouches, first.CollModule.polygon, second.CollModule.polygon);

            touches = touches.Concat(otherTouches
                .Select(t => new EdgeTouch(t.vertex, new LineSegment(t.edge.End, t.edge.Start)))
                .ToList()).ToList();

            if (touches.Count < 1)
                return;
            
            Vector2 impulse = Vector2.Zero;
            int touchCount = 0;
            foreach (var item in touches)
            {
                Vector2 touchNormal = item.edge.Normal;

                float velocityAlongNormal = Vector2.Dot(second.velocity - first.velocity, touchNormal);
                if (velocityAlongNormal > 0)
                    continue;

                Vector2 tempImpulse = GetImpulse(touchNormal, velocityAlongNormal, first, second);
                if (tempImpulse.LengthSquared() > ZeroThreshold)
                {
                    impulse += tempImpulse;
                    touchCount++;
                }
            }

            if (impulse == Vector2.Zero || touchCount == 0)
                return;

            first.velocity -= impulse / touchCount / first.Mass;
            second.velocity += impulse / touchCount / second.Mass;
        }
        private static void HandleOthers(Rigidbody rb, Collider other)
        {
            List<EdgeTouch> touches = GetTouches(rb.CollModule, other);
            List<EdgeTouch> otherTouches = GetTouches(other, rb.CollModule);
            
            List<CornerTouch> corners = CornerTouch.ExtractFrom(touches);
            List<CornerTouch> otherCorners = CornerTouch.ExtractFrom(otherTouches);

            if (corners.Count > 1)
                touches = touches.Concat(CornerTouch.FindCommonEdges(corners)).ToList();
            else
            {
                if (corners.Count == 1)
                    otherTouches = otherTouches.Concat(EdgeTouch.FromSingleCorner(corners, otherCorners)).ToList();
                if (otherCorners.Count == 1)
                    touches = touches.Concat(EdgeTouch.FromSingleCorner(otherCorners, corners)).ToList();
            }

            EdgeTouch.Refine(touches, other.polygon.GetEdges());
            EdgeTouch.Refine(otherTouches, rb.CollModule.polygon.GetEdges());
            EdgeTouch.Adjust(touches, otherTouches, rb.CollModule.polygon, other.polygon);
           
            List<EdgeTouch> allTouches = touches.Concat(otherTouches
                .Select(t => new EdgeTouch(t.vertex, new LineSegment(t.edge.End, t.edge.Start)))
                .ToList()).ToList();

            if (allTouches.Count < 1)
                return;

            Vector2 impulse = Vector2.Zero;
            int touchCount = 0;
            foreach (var item in allTouches)
            {
                Vector2 touchNormal = item.edge.Normal;

                float velocityAlongNormal = Vector2.Dot(rb.velocity, touchNormal);
                if (velocityAlongNormal > ZeroThreshold)
                    continue;

                Vector2 linearImpulse = GetSingleImpulse(touchNormal, velocityAlongNormal, rb);

                if (linearImpulse.LengthSquared() > ZeroThreshold)
                {
                    impulse += linearImpulse;
                    touchCount++;
                }

                if (velocityAlongNormal.Abs() < 0.1)
                    impulse += velocityAlongNormal * touchNormal;
            }

            Console.WriteLine(string.Join("\n", allTouches.Select(t => $"1v: {t.vertex} e: {t.edge} n: {t.edge.Normal}")));

            foreach (var item in otherTouches)
            {
                Vector2 r = item.vertex - rb.Owner.IntegerPosition;
                Vector2 vertexVelocity = r.ScalarProduct(rb.AngularVelocity);
                Vector2 normal = -item.edge.Normal;


                float velocityAlongNormal = Vector2.Dot(vertexVelocity + rb.velocity, normal);
                if (velocityAlongNormal > ZeroThreshold)
                    continue;

                Console.WriteLine($"velocity: {rb.velocity} vertex: {item.vertex}");
                rb.AngularVelocity += vertexVelocity.Cross(r) / r.LengthSquared() * (1.0f + rb.Bounciness);
            }
            Console.WriteLine();

            if (impulse.Length() < ZeroThreshold || touchCount == 0)
                return;

            rb.velocity += impulse / touchCount / rb.Mass;
        }

        private static Vector2 GetImpulse(Vector2 touchNormal, float velocityAlongNormal, Rigidbody first, Rigidbody second)
        {
            float e = Math.Max(second.Bounciness, first.Bounciness);

            float j = -(1 + e) * velocityAlongNormal;
            j /= 1 / first.Mass + 1 / second.Mass;

            return j * touchNormal;
        }
        private static Vector2 GetSingleImpulse(Vector2 touchNormal, float velocityAlongNormal, Rigidbody rb)
        {
            float e = rb.Bounciness;
            float j = -(1 + e) * velocityAlongNormal;
            j /= 1 / rb.Mass;

            return j * touchNormal;
        }
        private void ApplyGravity() => AddForce(new(0, (Gravity * Mass * GravityScale) * (FixedDelta * 200)));
        private void ApplyFriction(float frictValue)
        {
            float deltaFrict = (frictValue / Mass) * FixedDelta * 4;
            float frictionThreshold = 0.001f;

            if (velocity.AbsX() > frictionThreshold)
                velocity.X += velocity.X < 0 ? deltaFrict : -deltaFrict;
            else velocity.X = 0;

            if (velocity.AbsY() > frictionThreshold)
                velocity.Y += velocity.Y < 0 ? deltaFrict : -deltaFrict;
            else velocity.Y = 0;

            velocity.X = (float)Math.Round(velocity.X, 3);
            velocity.Y = (float)Math.Round(velocity.Y, 3);
        }

        private void OnColliderDestruct() => Dispose();
        protected override void Destruct() => CollModule.OnCheckFinish -= Update;
        
        private static List<EdgeTouch> GetPointsOnEdges(List<LineSegment> edges, List<Vector2> vertices) => 
            (from e in edges 
            from v in vertices
            where e.IsPointBetween(v, Tolerance)
            select new EdgeTouch(v, e)).ToList();
        private static List<EdgeTouch> GetTouches(Collider edgesColl, Collider verticesColl)
        {
            List<LineSegment> edges = edgesColl.polygon.GetEdges()
                .Select(e => new LineSegment(e.Start.Rounded(), e.End.Rounded()))
                .ToList();

            List<Vector2> vertices = verticesColl.polygon.Vertices
                .Select(v => (v + verticesColl.polygon.position).Rounded())
                .ToList();

            return GetPointsOnEdges(edges, vertices);
        }
    }
}