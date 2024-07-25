using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Types;
using System.Reflection;

namespace Engine.Modules
{
    public class Rigidbody : ObjectModule
    {
        public float mass = 1;
        public float gravityScale = 1;
        public float bounciness = 0.2f;
        public float friction = 0.3f;
        public float airFriction = 0.3f;

        public float angle = 0;
        public float torque = 0;
        public float angularVelocity = 0;
        public float angularAcceleration = 0;
        public float momentOfInertia = 1;

        public Vector2 forces = Vector2.Zero;
        public Vector2 velocity = Vector2.Zero;
        public Collider collider;

        public const float Gravity = 9.81f;
        public const float FixedDelta = 1.0f / 180.0f;
        private float accumulatedTime = 0.0f;

        public Rigidbody(GameObject owner) : base(owner)
        {
            if (owner.TryGetModule<Collider>(out var module))
                collider = module;
            else
                collider = owner.AddModule<Collider>();

            collider.OnRemoved += OnColliderDestruct;
            collider.OnCheckFinish += Update;
        }

        private void OnColliderDestruct() => Owner.RemoveModule(this);
        protected override void Destruct() => collider.OnCheckFinish -= Update;

        public void AddForce(Vector2 force) => forces += force;


        private void Update(GameTime gameTime)
        {
            accumulatedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            while (accumulatedTime >= FixedDelta)
            {
                //ApplyGravity();

                velocity += forces / mass * FixedDelta;
                forces = Vector2.Zero;

                foreach (var item in collider.Intersections)
                {
                    if (item.Owner.TryGetModule(out Rigidbody otherRb))
                        HandlePhysical(this, otherRb);
                    else
                        HandleOthers(item);
                }

                ApplyFriction(airFriction);

                Owner.position += velocity;
                accumulatedTime -= FixedDelta;
            }
        }

        private static void HandlePhysical(Rigidbody first, Rigidbody second)
        {
            List<(Vector2 point, LineSegment edge)> edgeTouches = GetTouchPointsWithEdgesFor(second.collider, first.collider);
            if (edgeTouches.Count < 1)
                return;

            foreach (var (_, edge) in edgeTouches)
            {
                Vector2 touchNormal = edge.Normal;

                float velocityAlongNormal = Vector2.Dot(second.velocity - first.velocity, touchNormal);
                if (velocityAlongNormal > 0)
                    continue;

                Vector2 impulse = GetImpulse(touchNormal, velocityAlongNormal, first, second);

                first.velocity -= impulse / first.mass;
                second.velocity += impulse / second.mass;
            }
        }
        private void HandleOthers(Collider other)
        {
            bool isOtherTouches = false;

            List<(Vector2 point, LineSegment edge)> edgeTouches = GetTouchPointsWithEdgesFor(collider, other);
            
            if(edgeTouches.Count < 1)
            {
                edgeTouches = GetTouchPointsWithEdgesFor(other, collider);
                isOtherTouches = true;

                if (edgeTouches.Count < 1)
                    return;
            }

            foreach (var (_, edge) in edgeTouches)
            {
                Vector2 touchNormal = !isOtherTouches ? edge.Normal : -edge.Normal;

                float velocityAlongNormal = Vector2.Dot(velocity, touchNormal);
                if (velocityAlongNormal > 0)
                    continue;

                float e = bounciness;
                float j = -(1 + e) * velocityAlongNormal;
                j /= 1 / mass;

                Vector2 impulse = j * touchNormal;
                velocity += impulse / mass;
            }
        }

        private static Vector2 GetImpulse(Vector2 touchNormal, float velocityAlongNormal, Rigidbody first, Rigidbody second)
        {
            float e = Math.Min(second.bounciness, first.bounciness);
            float j = -(1 + e) * velocityAlongNormal;
            j /= 1 / first.mass + 1 / second.mass;

            return j * touchNormal;
        }
        private void ApplyGravity() => AddForce(new(0, (Gravity * mass) * (FixedDelta * 200)));
        private void ApplyFriction(float frictValue)
        {
            float deltaFrict = (frictValue / mass) * FixedDelta;

            if (Math.Abs(velocity.X) > 0)
                velocity.X += velocity.X < 0 ? deltaFrict : -deltaFrict;

            if (Math.Abs(velocity.Y) > 0)
                velocity.Y += velocity.Y < 0 ? deltaFrict : -deltaFrict;
        }


        public static List<Point> GetTouchPoints(Collider coll1, Collider coll2)
        {
            LineSegment[] e1 = coll1.polygon.GetEdges().ToArray();
            LineSegment[] e2 = coll2.polygon.GetEdges().ToArray();

            Vector2[] v1 = coll1.polygon.Vertices.Select(v => v + coll1.polygon.FlooredPosition).ToArray();
            Vector2[] v2 = coll2.polygon.Vertices.Select(v => v + coll2.polygon.FlooredPosition).ToArray();


            List<Point> touchPoints = new();

            touchPoints = touchPoints.Concat(GetVertsOnSegments(e1, v2)).ToList();
            touchPoints = touchPoints.Concat(GetVertsOnSegments(e2, v1)).ToList();

            return touchPoints.Distinct().ToList();
        }
        public static List<Point> GetVertsOnSegments(LineSegment[] edges, Vector2[] vertices)
        {
            List<Point> points = new();

            foreach (var e in edges)
            {
                foreach (var v in vertices)
                {
                    Point p = v.ToPoint();

                    if (e.IsPointOn(p))
                    {
                        points.Add(p);
                    }
                }
            }
            return points;
        }

        public static List<(Vector2 touch, LineSegment edge)> GetVertsOnSegmentsWithEdges(LineSegment[] edges, Vector2[] vertices)
        {
            List<(Vector2 touch, LineSegment edge)> points = new();

            foreach (var e in edges)
            {
                foreach (var v in vertices)
                {
                    if (e.IsPointOn(v, 100))
                    {
                        points.Add((v, e));
                    }
                }
            }
            return points;
        }

        public static List<(Vector2 touch, LineSegment edge)> GetTouchPointsWithEdgesFor(Collider edges, Collider vertices)
        {
            LineSegment[] e1 = edges.polygon.GetEdges().ToArray();
            Vector2[] v2 = vertices.polygon.Vertices.Select(v => v + vertices.polygon.position).ToArray();

            return GetVertsOnSegmentsWithEdges(e1, v2).ToList();
        }
    }
}