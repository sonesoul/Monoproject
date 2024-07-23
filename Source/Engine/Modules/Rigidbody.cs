using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Types;
using Extensions.Extensions;

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

            collider.OnDestruct += OnColliderDestruct;

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
                ApplyGravity();

                velocity += forces / mass * FixedDelta;
                forces = Vector2.Zero;

                foreach (var item in collider.Intersections)
                {
                    if (item.Owner.TryGetModule(out Rigidbody otherRb))
                        HandleRigidbodyCollision(otherRb);
                    else
                        HandleStaticCollision(item);
                }

                ApplyFriction(airFriction);

                Owner.position += velocity;
                accumulatedTime -= FixedDelta;
            }
        }

        private void HandleRigidbodyCollision(Rigidbody other)
        {
            List<(Point point, LineSegment edge)> touchPoints = GetTouchPointsWithEdges(collider, other.collider);
            if (touchPoints.Count < 1)
                return;

            Vector2 finalImpulse = Vector2.Zero;

            foreach (var p in touchPoints)
            {
                Vector2 point = p.point.ToVector2();
                Vector2 touchNormal = other.Owner.position - point;

                touchNormal.Normalize();

                float velocityAlongNormal = Vector2.Dot(other.velocity, touchNormal);

                if (velocityAlongNormal > -1)
                    return;

                float e = Math.Min(bounciness, other.bounciness);
                float j = -(1 + e) * velocityAlongNormal;
                j /= 1 / mass + 1 / other.mass;

                Vector2 impulse = j * touchNormal;
                finalImpulse += impulse / touchPoints.Count;
            }

            velocity -= finalImpulse / mass;
            other.velocity += finalImpulse / other.mass;
        }
        private void HandleStaticCollision(Collider other)
        {

            List<Point> touchPoints = GetTouchPoints(collider, other);
            if (touchPoints.Count < 1)
                return;

            Vector2 averageTouch = Vector2.Zero;
            foreach (var p in touchPoints)
                averageTouch += p.ToVector2();
            averageTouch /= touchPoints.Count;


            Vector2 touchNormal = Owner.position - averageTouch;
            touchNormal.Normalize();

            float velocityAlongNormal = Vector2.Dot(velocity, touchNormal);
            if (velocityAlongNormal > 0)
                return;

            float e = bounciness;
            float j = -(1 + e) * velocityAlongNormal;
            j /= 1 / mass;


            Vector2 impulse = j * touchNormal;            
            velocity += impulse / mass;
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
            LineSegment[] s1 = coll1.polygon.GetEdges().ToArray();
            LineSegment[] s2 = coll2.polygon.GetEdges().ToArray();

            Vector2[] v1 = coll1.polygon.Vertices.Select(v => v + coll1.polygon.IntegerPosition).ToArray();
            Vector2[] v2 = coll2.polygon.Vertices.Select(v => v + coll2.polygon.IntegerPosition).ToArray();


            List<Point> touchPoints = new();

            touchPoints = touchPoints.Concat(GetVertsOnSegments(s1, v2)).ToList();
            touchPoints = touchPoints.Concat(GetVertsOnSegments(s2, v1)).ToList();

            return touchPoints.Distinct().ToList();
        }
        public static List<Point> GetVertsOnSegments(LineSegment[] segments, Vector2[] vertices)
        {
            List<Point> points = new();

            foreach (var s in segments)
            {
                foreach (var v in vertices)
                {
                    Point p = v.ToPoint();

                    if (s.IsPointOn(p))
                    {
                        points.Add(p);
                    }
                }
            }
            return points;
        }

        public static List<(Point touch, LineSegment edge)> GetVertsOnSegmentsWithEdges(LineSegment[] segments, Vector2[] vertices)
        {
            List<(Point touch, LineSegment edge)> points = new();

            foreach (var s in segments)
            {
                foreach (var v in vertices)
                {
                    Point p = v.ToPoint();

                    if (s.IsPointOn(p))
                    {
                        points.Add((p, s));
                    }
                }
            }
            return points;
        }
        public static List<(Point touch, LineSegment edge)> GetTouchPointsWithEdges(Collider coll1, Collider coll2)
        {
            LineSegment[] s1 = coll1.polygon.GetEdges().ToArray();
            LineSegment[] s2 = coll2.polygon.GetEdges().ToArray();

            Vector2[] v1 = coll1.polygon.Vertices.Select(v => v + coll1.polygon.IntegerPosition).ToArray();
            Vector2[] v2 = coll2.polygon.Vertices.Select(v => v + coll2.polygon.IntegerPosition).ToArray();


            List<(Point, LineSegment)> touchPoints = new();

            touchPoints = touchPoints.Concat(GetVertsOnSegmentsWithEdges(s1, v2)).ToList();
            touchPoints = touchPoints.Concat(GetVertsOnSegmentsWithEdges(s2, v1)).ToList();

            return touchPoints.Distinct().ToList();
        }
    }
}