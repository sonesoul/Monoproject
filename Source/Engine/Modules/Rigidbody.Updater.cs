using GlobalTypes.Events;
using GlobalTypes;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Modules
{
    public partial class Rigidbody
    {
        [Init(nameof(Init))]
        private static class Updater
        {
            private static List<Rigidbody> bodies = new();
            private static Queue<Rigidbody> updateQueue = new();

            private static void Init()
            {
                FrameEvents.EndUpdate.Add(() =>
                {
                    BatchContacts();
                    UpdateCollisions();
                }, EndUpdateOrders.RigidbodyUpdater);

                FrameEvents.PostDraw.Add(() =>
                {
                    ApplyGravity();
                    ApplyForces();

                    ApplyVelocity();
                });
            }

            public static void Register(Rigidbody rb)
            {
                if (!bodies.Contains(rb))
                    bodies.Add(rb);
            }
            public static void Unregister(Rigidbody rb)
            {
                if (bodies.Contains(rb))
                    bodies.Remove(rb);
            }

            private static void UpdateCollisions()
            {
                List<Rigidbody> handled = new();
                var sorted = bodies.OrderByDescending(b => GetPriority(b)).ToList();

                foreach (var body in sorted)
                {
                    if (handled.Contains(body) || !body.UsedCollider.Intersects)
                        continue;

                    body.UpdatePhysics();

                    while (updateQueue.Count > 0)
                    {
                        var queuedBody = updateQueue.Dequeue();

                        queuedBody.UpdatePhysics();

                        handled.Add(queuedBody);
                    }
                }
            }

            private static void BatchContacts() => bodies.PForEach(b => b?.Batch());
            private static void ApplyGravity() => bodies.PForEach(b => b?.ApplyGravity());
            private static void ApplyVelocity() => bodies.PForEach(b => b?.ApplyVelocity());
            private static void ApplyForces() => bodies.PForEach(b => b?.ApplyForces());

            private static int GetPriority(Rigidbody rb)
            {
                int priority = 0;

                priority += rb.UsedCollider.Intersections.Count;

                if (rb.BodyType == BodyType.Static)
                    priority += 1000;

                priority += (int)(rb.velocity.Length());


                return priority;
            }
        }
    }
}