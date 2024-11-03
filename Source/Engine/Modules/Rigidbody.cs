using Engine.Types;
using GlobalTypes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Modules
{
    public enum BodyType
    {
        Dynamic,
        Kinematic,
        Static
    }

    public partial class Rigidbody : ObjectModule
    {
        public struct Contact
        {
            public readonly struct ContactBatch
            {
                public List<Contact> ThisContacts { get; init; }
                public Rigidbody ThisRigidbody { get; init; }
                public Vector2 ThisMTV { get; init; }

                public List<Contact> OtherContacts { get; init; }
                public Rigidbody OtherRigidbody { get; init; }
                public Vector2 OtherMTV { get; init; }

                public ContactBatch(List<Contact> thisContacts, List<Contact> otherContacts, Rigidbody thisRb, Rigidbody otherRb)
                {
                    ThisContacts = thisContacts;
                    ThisRigidbody = thisRb;

                    OtherContacts = otherContacts;
                    OtherRigidbody = otherRb;

                    var mtv = thisRb.UsedCollider.GetMTV(otherRb.UsedCollider);

                    ThisMTV = mtv;
                    OtherMTV = -mtv;
                }
            }

            public readonly Vector2 Normal => Edge.UnitNormal;
            public readonly Vector2 Vertex => Corner.CommonVertex;
            public readonly Vector2 MovedVertex => Edge.ClosestPoint(Corner.CommonVertex);

            public Corner Corner { get; set; }
            public LineSegment Edge { get; set; }

            public Contact(Corner corner, LineSegment edge)
            {
                Corner = corner;
                Edge = edge;
            }

            public static List<Contact> Detect(Polygon edgesPoly, Polygon vertsPoly)
            {
                List<Contact> contacts = new();

                foreach (var vert in vertsPoly.WorldVertices)
                {
                    if (edgesPoly.ContainsPoint(vert))
                    {
                        var mtv = edgesPoly.GetMTV(vertsPoly);

                        contacts.Add(
                            new(
                                Corner.FromVertex(vert, vertsPoly),
                                edgesPoly.ClosestNormalEdge(mtv) + edgesPoly.IntegerPosition));
                    }
                }

                return contacts;
            }

            public static ContactBatch Batch(Rigidbody first, Rigidbody second)
            {
                var firstShape = first.UsedCollider.Shape;
                var secondShape = second.UsedCollider.Shape;

                List<Contact> contacts = Detect(firstShape, secondShape);
                List<Contact> otherContacts = Detect(secondShape, firstShape);

                return new(otherContacts, contacts, first, second);
            }
        }
        public struct Corner
        {
            public Vector2 CommonVertex { get; set; }

            public LineSegment FirstEdge { get; set; }
            public LineSegment SecondEdge { get; set; }

            public Corner(Vector2 vertex, LineSegment edge1, LineSegment edge2)
            {
                CommonVertex = vertex;
                FirstEdge = edge1;
                SecondEdge = edge2;
            }

            public static Corner FromVertex(Vector2 vertex, Polygon poly)
            {
                List<LineSegment> corners = poly.WorldEdges.Where(e => e.ContainsPoint(vertex)).ToList();
                return new Corner(vertex, corners[0], corners[1]);
            }

            public readonly override string ToString() => CommonVertex.ToString();
        }

        public static Vector2 Gravity { get; set; } = new Vector2(0, 9.81f) * 50; // if 1 meter == 50 pixels

        public float Mass { get; set; } = 1;
        public float Bounciness { get; set; } = 0.0f;
        public float Windage { get; set; } = 0.3f;

        public Vector2 VelocityScale { get; set; } = new(1, 1);
        public Vector2 GravityScale { get; set; } = new(3, 3);
        public Vector2 MaxVelocity { get; set; } = new(-1, -1);

        public Collider UsedCollider { get; set; }
        public BodyType BodyType { get; set; }
        
        public Vector2 velocity = Vector2.Zero;
        private Vector2 forces = Vector2.Zero;

        private Queue<Contact.ContactBatch> contactBatches = new();
        private List<Rigidbody> suppresedPhysics = new();

        private static float Delta => FrameInfo.FixedDeltaTime;

        public Rigidbody(ModularObject owner = null) : base(owner) { }
        protected override void PostConstruct()
        {
            if (Owner.TryGetModule<Collider>(out var module))
                UsedCollider = module;
            else
                UsedCollider = Owner.AddModule<Collider>();

            Updater.Register(this);

            UsedCollider.OnDispose += () => UsedCollider = null;
        }

        public void AddForce(Vector2 force) => forces += force;

        private void ApplyForces()
        {
            if (BodyType == BodyType.Static)
                return;

            velocity += forces / Mass * Delta * VelocityScale;
            forces = Vector2.Zero;
        }
        private void ApplyVelocity() => Owner.Position += velocity;
        private void ApplyGravity() => AddForce(Gravity * GravityScale * Delta);

        private void Batch()
        {
            if (UsedCollider == null)
                return;
            
            suppresedPhysics.Clear();

            foreach (var item in UsedCollider.Intersections)
            {
                if (!item.Owner.TryGetModule<Rigidbody>(out var itemRb))
                    continue;

                var batch = Contact.Batch(this, itemRb);

                if (!contactBatches.Contains(batch))
                    contactBatches.Enqueue(batch);
            }
        }
        
        private void UpdatePhysics()
        {
            if (UsedCollider == null)
                return;

            //iterating collided objects
            while (contactBatches.Count > 0) 
            {
                var batch = contactBatches.Dequeue();

                if (!suppresedPhysics.Contains(batch.OtherRigidbody))
                    HandleCollision(batch);
            }
        }
        private static void HandleCollision(Contact.ContactBatch batch)
        {
            List<Contact> totalContacts = batch.ThisContacts.Concat(batch.OtherContacts).ToList();

            Rigidbody thisRb = batch.ThisRigidbody;
            Rigidbody otherRb = batch.OtherRigidbody;
            
            if (thisRb.BodyType == BodyType.Static && otherRb.BodyType == BodyType.Static)
                return;

            Vector2 impulse = Vector2.Zero;
            int touchCount = 0;

            for (int i = 0; i < totalContacts.Count; i++)
            {
                bool isOtherCheck = i >= batch.ThisContacts.Count;
                Contact item = totalContacts[i];

                Vector2 normal = item.Normal;

                if (isOtherCheck)
                    normal = -normal;

                float velocityAlongNormal = Vector2.Dot(otherRb.velocity - thisRb.velocity, normal);

                if (velocityAlongNormal > 0)
                    continue;

                Vector2 normalImpulse = GetImpulse(normal, velocityAlongNormal, thisRb, otherRb);

                impulse += normalImpulse;
                touchCount++;
            }

            if (touchCount > 0)
            {
                if (thisRb.BodyType == BodyType.Dynamic)
                {
                    thisRb.velocity -= impulse / touchCount;
                }
                if (otherRb.BodyType == BodyType.Dynamic)
                {
                    otherRb.velocity += impulse / touchCount;
                }
            }

            ResolveCollision(batch);
            otherRb.SuppressUpdate(thisRb);
        }
        
        private static void ResolveCollision(Contact.ContactBatch batch)
        {
            Vector2 mtv = batch.ThisRigidbody.UsedCollider.GetMTV(batch.OtherRigidbody.UsedCollider);
            Rigidbody rb;

            if (batch.ThisRigidbody.BodyType == BodyType.Dynamic)
            {
                rb = batch.ThisRigidbody;
                mtv = -mtv;
            }
            else
            {
                rb = batch.OtherRigidbody;
            }

            mtv.Round();
            var mtvLength = mtv.Length();

            if (mtvLength >= 2f)
            {
                rb.Owner.Position += mtv / 2;
                rb.Owner.Position = rb.Owner.Position.Rounded();
                
                rb.UsedCollider.UpdateShape();
                
            }
        }
        private void SuppressUpdate(Rigidbody rb) => suppresedPhysics.Add(rb);

        private static Vector2 GetImpulse(Vector2 touchNormal, float velocityAlongNormal, Rigidbody first, Rigidbody second)
        {
            float e = Math.Max(first.Bounciness, second.Bounciness);
            float j = -(1 + e) * velocityAlongNormal;

            if (first.BodyType == BodyType.Static)
            {
                j /= 1 / second.Mass;
            }
            else if (second.BodyType == BodyType.Static)
            {
                j /= 1 / first.Mass;
            }
            else
            {
                j /= 1 / first.Mass + 1 / second.Mass;
            }

            return j * touchNormal;
        }
        
        protected override void PostDispose()
        {
            Updater.Unregister(this);

            contactBatches.Clear();
            suppresedPhysics.Clear();

            contactBatches = null;
            suppresedPhysics = null;
            UsedCollider = null;
        }
    }
}