using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Types;
using Monoproject;

namespace Engine.Modules
{
    public class Rigidbody : ObjectModule
    {
        #region Fields
        public float Mass { get; set; } = 1;
        public float GravityScale { get; set; } = 1;
        public float Bounciness { get; set; } = 0.2f;
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

            CollModule.OnRemoved += OnColliderDestruct;
            CollModule.OnCheckFinish += Update;
        }

        public void AddForce(Vector2 force) => forces += force;
        public void SetCollider(Collider newCollider)
        {
            CollModule = Owner.ReplaceModule(newCollider);
            CollModule.OnRemoved += OnColliderDestruct;
            CollModule.OnCheckFinish += Update;
        }

        private void Update(GameTime gameTime)
        {
            updateTimeBuffer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            while (updateTimeBuffer >= FixedDelta)
            {
                velocity += forces / Mass * FixedDelta;
                forces = Vector2.Zero;

                foreach (var item in CollModule.Intersections)
                {
                    if (item.Owner.TryGetModule(out Rigidbody otherRb))
                        HandlePhysical(this, otherRb);
                    else
                        HandleOthers(this, item);
                }

                ApplyGravity();
                ApplyFriction(Windage);

                Main.Instance.player.GetModule<Collider>().info = velocity.ToString();

                Owner.position += velocity;
                updateTimeBuffer -= FixedDelta;
            }
        }
        public static void HandlePhysical(Rigidbody first, Rigidbody second)
        {
            List<EdgeTouch> touches = GetTouches(second.CollModule, first.CollModule);
            List<CornerTouch> cornerTouches = CornerTouch.ExtractFrom(touches);

            if (cornerTouches.Count > 1)
                touches = touches.Concat(CornerTouch.FindCommonEdges(cornerTouches)).ToList();
            else if(cornerTouches.Count == 1)
                touches = touches.Concat(EdgeTouch.FromSingleCorner(cornerTouches, CornerTouch.ExtractFrom(GetTouches(first.CollModule, second.CollModule)))).ToList();

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

                Vector2 tempImpulse = GetSingleImpulse(touchNormal, velocityAlongNormal, rb);

                if (tempImpulse.LengthSquared() > ZeroThreshold)
                {
                    impulse += tempImpulse;
                    touchCount++;
                }

                if (velocityAlongNormal.Abs() < 0.1)
                    impulse += velocityAlongNormal * touchNormal;
            }

            if (impulse.Length() < ZeroThreshold || touchCount == 0)
                return;

            rb.velocity += impulse / touchCount / rb.Mass;
        }

        private static Vector2 GetImpulse(Vector2 touchNormal, float velocityAlongNormal, Rigidbody first, Rigidbody second)
        {
            float e = Math.Min(second.Bounciness, first.Bounciness);

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



        private void OnColliderDestruct() => Owner.RemoveModule(this);
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