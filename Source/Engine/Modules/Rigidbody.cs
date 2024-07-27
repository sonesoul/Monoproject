using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
        public const float FixedDelta = 1.0f / 120.0f;
        public const float ZeroTheshold = 0.02f;

        private float updateTimeBuffer = 0.0f;
        #endregion

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
                //ApplyGravity();

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
            List<(Vector2 point, LineSegment edge)> edgeTouches = GetTouches(second.collider, first.collider);

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
        private static void HandleOthers(Rigidbody rb, Collider other)
        {
            bool isOtherTouches = false;
            
            List<(Vector2 point, LineSegment edge)> edgeTouches = GetTouches(rb.collider, other);

            if (edgeTouches.Count < 1)
            {
                edgeTouches = GetTouches(other, rb.collider);
                isOtherTouches = true;

                if (edgeTouches.Count < 1)
                    return;
            }

            foreach (var (_, edge) in edgeTouches)
            {
                Vector2 touchNormal = !isOtherTouches ? edge.Normal : -edge.Normal;

                float velocityAlongNormal = Vector2.Dot(rb.velocity, touchNormal);
                if (velocityAlongNormal > 1)
                    continue;

                float e = rb.bounciness;
                float j = -(1 + e) * velocityAlongNormal;
                j /= 1 / rb.mass;

                Vector2 impulse = j * touchNormal;
                
                rb.velocity += impulse / rb.mass;
            }
        }

        private static Vector2 GetImpulse(Vector2 touchNormal, float velocityAlongNormal, Rigidbody first, Rigidbody second)
        {
            float e = Math.Min(second.bounciness, first.bounciness);
            float j = -(1 + e) * velocityAlongNormal;
            j /= 1 / first.mass + 1 / second.mass;

            return j * touchNormal;
        }
        private void ApplyGravity() => AddForce(new(0, (Gravity * mass * gravityScale) * (FixedDelta * 200)));
        private void ApplyFriction(float frictValue)
        {
            float deltaFrict = (frictValue / mass) * FixedDelta * 4;

            if (velocity.AbsX() > ZeroTheshold)
                velocity.X += velocity.X < 0 ? deltaFrict : -deltaFrict;
            else velocity.X = 0;

            if (velocity.AbsY() > ZeroTheshold)
                velocity.Y += velocity.Y < 0 ? deltaFrict : -deltaFrict;
            else velocity.Y = 0;

            velocity.X = (float)Math.Round(velocity.X, 3);
            velocity.Y = (float)Math.Round(velocity.Y, 3);
        }

        private void OnColliderDestruct() => Owner.RemoveModule(this);
        protected override void Destruct() => collider.OnCheckFinish -= Update;
        
        public static List<(Vector2 touch, LineSegment edge)> GetVertsOnEdges(LineSegment[] edges, Vector2[] vertices)
        {
            List<(Vector2 touch, LineSegment edge)> vertsOnEdges = new();

            foreach (var edge in edges)
            {
                foreach (var vertex in vertices)
                {
                    if (edge.IsPointOn(vertex, 3))
                        vertsOnEdges.Add((vertex, edge));
                }
            }

            return vertsOnEdges;
        }
        public static List<(Vector2 touch, LineSegment edge)> GetTouches(Collider edgesColl, Collider verticesColl)
        {
            LineSegment[] edges = edgesColl.polygon.GetEdges().Select(e => new LineSegment(e.Start.Rounded(), e.End.Rounded())).ToArray();
            Vector2[] vertices = verticesColl.polygon.Vertices.Select(v => (v + verticesColl.polygon.Position).Rounded()).ToArray();

            return GetVertsOnEdges(edges, vertices).ToList();
        }
    }
}