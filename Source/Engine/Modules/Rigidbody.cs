using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Types;
using Microsoft.Xna.Framework.Input;

namespace Engine.Modules
{
    public class Rigidbody : ObjectModule
    {
        #region Fields
        public float mass = 1;
        public float gravityScale = 1;
        public float bounciness = 0.2f;
        public float friction = 0.3f;
        public float windage = 0.3f;

        public float angle = 0;
        public float torque = 0;
        public float angularVelocity = 0;
        public float angularAcceleration = 0;
        public float momentOfInertia = 1;

        public Vector2 forces = Vector2.Zero;
        public Vector2 velocity = Vector2.Zero;
        public Collider collider;

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
        }

        public Rigidbody(GameObject owner) : base(owner)
        {
            if (owner.TryGetModule<Collider>(out var module))
                collider = module;
            else
                collider = owner.AddModule<Collider>();

            collider.OnRemoved += OnColliderDestruct;
            collider.OnCheckFinish += Update;
        }

        public void AddForce(Vector2 force) => forces += force;
        
        private void Update(GameTime gameTime)
        {
            updateTimeBuffer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            while (updateTimeBuffer >= FixedDelta)
            {
                ApplyGravity();

                velocity += forces / mass * FixedDelta;
                forces = Vector2.Zero;

                foreach (var item in collider.Intersections)
                {
                    if (item.Owner.TryGetModule(out Rigidbody otherRb))
                        HandlePhysical(this, otherRb);
                    else
                        HandleOthers(this, item);
                }

                ApplyFriction(windage);

                Owner.position += velocity;
                updateTimeBuffer -= FixedDelta;
            }
        }
        public static void HandlePhysical(Rigidbody first, Rigidbody second)
        {
            List<EdgeTouch> touches = GetTouches(second.collider, first.collider);
            List<CornerTouch> cornerTouches = ExtractCornerTouches(touches);

            if (cornerTouches.Count > 1)
                touches = touches.Concat(GetCommonEdges(cornerTouches)).ToList();
            else if(cornerTouches.Count == 1)
                touches = touches.Concat(GetEdgeTouchesFromCorner(cornerTouches, ExtractCornerTouches(GetTouches(first.collider, second.collider)))).ToList();

            List<EdgeTouch> allTouches = touches;

            
            first.collider.info = string.Join("\n", allTouches.Select(t => $"v: {t.vertex}\n e: {t.edge}\n").ToList());
            second.collider.info = string.Join("\n", allTouches.Select(t => $"v: {t.vertex}\n e: {t.edge}\n").ToList());

            if (allTouches.Count < 1)
                return;

            Vector2 impulse = Vector2.Zero;
            int touchCount = 0;
            foreach (var item in allTouches)
            {
                Vector2 touchNormal = item.edge.Normal;

                float velocityAlongNormal = Vector2.Dot(second.velocity - first.velocity, touchNormal);
                if (velocityAlongNormal > 0)
                    continue;

                Vector2 tempImpulse = GetImpulse(touchNormal, velocityAlongNormal, first, second);
                if (Vector2.Dot(tempImpulse, tempImpulse) > ZeroThreshold)
                    touchCount++;

                impulse += tempImpulse;
            }

            if (impulse == Vector2.Zero || touchCount == 0)
                return;

            first.velocity -= impulse / touchCount / first.mass;
            second.velocity += impulse / touchCount / second.mass;
        }
        private static void HandleOthers(Rigidbody rb, Collider other)
        {
            List<EdgeTouch> touches = GetTouches(rb.collider, other);
            List<EdgeTouch> otherTouches = GetTouches(other, rb.collider);

            List<CornerTouch> corners = ExtractCornerTouches(touches);
            List<CornerTouch> otherCorners = ExtractCornerTouches(otherTouches);

            if (corners.Count > 1)
                touches = touches.Concat(GetCommonEdges(corners)).ToList();
            else
            {
                if (corners.Count == 1)
                    otherTouches = otherTouches.Concat(GetEdgeTouchesFromCorner(corners, otherCorners)).ToList();
                if (otherCorners.Count == 1)
                    touches = touches.Concat(GetEdgeTouchesFromCorner(otherCorners, corners)).ToList();
            }


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
                if (velocityAlongNormal > 0)
                    continue;

                Vector2 tempImpulse = GetSingleImpulse(touchNormal, velocityAlongNormal, rb);
                if (Vector2.Dot(tempImpulse, tempImpulse) > ZeroThreshold)
                    touchCount++;

                impulse += tempImpulse;
            }

            if (impulse == Vector2.Zero || touchCount == 0)
                return;
            
            rb.velocity += impulse / touchCount / rb.mass;
        }

        private static List<CornerTouch> ExtractCornerTouches(List<EdgeTouch> items)
        {
            var resultPairs = new List<CornerTouch>();

            var groups = items.GroupBy(item => item.vertex);
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
                            
                            items.Remove(edges[i]);
                            items.Remove(edges[j]);
                        }
                    }
                }
            }

            return resultPairs;
        }
        private static List<EdgeTouch> GetCommonEdges(List<CornerTouch> corners)
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
                    

                    if(commonEdge != null)
                    {
                        result.Add(new(c1.commonVertex, commonEdge.Value));
                        result.Add(new(c2.commonVertex, commonEdge.Value));
                    }
                    
                }
            }
            return result;
        }
        private static List<EdgeTouch> GetEdgeTouchesFromCorner(List<CornerTouch> touches1, List<CornerTouch> touches2)
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
        
        private static Vector2 GetImpulse(Vector2 touchNormal, float velocityAlongNormal, Rigidbody first, Rigidbody second)
        {
            float e = Math.Min(second.bounciness, first.bounciness);
            
            float j = -(1 + e) * velocityAlongNormal;
            j /= 1 / first.mass + 1 / second.mass;

            return j * touchNormal;
        }
        private static Vector2 GetSingleImpulse(Vector2 touchNormal, float velocityAlongNormal, Rigidbody rb)
        {
            float e = rb.bounciness;
            float j = -(1 + e) * velocityAlongNormal;
            j /= 1 / rb.mass;

            return j * touchNormal;
        }
        private void ApplyGravity() => AddForce(new(0, (Gravity * mass * gravityScale) * (FixedDelta * 200)));
        private void ApplyFriction(float frictValue)
        {
            float deltaFrict = (frictValue / mass) * FixedDelta * 4;

            if (velocity.AbsX() > ZeroThreshold)
                velocity.X += velocity.X < 0 ? deltaFrict : -deltaFrict;
            else velocity.X = 0;

            if (velocity.AbsY() > ZeroThreshold)
                velocity.Y += velocity.Y < 0 ? deltaFrict : -deltaFrict;
            else velocity.Y = 0;

            velocity.X = (float)Math.Round(velocity.X, 3);
            velocity.Y = (float)Math.Round(velocity.Y, 3);
        }

        private void OnColliderDestruct() => Owner.RemoveModule(this);
        protected override void Destruct() => collider.OnCheckFinish -= Update;
        
        private static List<EdgeTouch> GetVertsOnEdges(LineSegment[] edges, Vector2[] vertices)
        {
            List<EdgeTouch> vertsOnEdges = new();

            foreach (var edge in edges)
            {
                foreach (var vertex in vertices)
                {
                    if (edge.IsPointBetween(vertex, Tolerance))
                        vertsOnEdges.Add(new(vertex, edge));
                }
            }

            return vertsOnEdges;
        }
        private static List<EdgeTouch> GetTouches(Collider edgesColl, Collider verticesColl)
        {
            LineSegment[] edges = edgesColl.polygon.GetEdges().Select(e => new LineSegment(e.Start.Rounded(), e.End.Rounded())).ToArray();
            Vector2[] vertices = verticesColl.polygon.Vertices.Select(v => (v + verticesColl.polygon.position).Rounded()).ToArray();

            return GetVertsOnEdges(edges, vertices).ToList();
        }
    }
}