using GlobalTypes.Collections;
using GlobalTypes.Events;
using GlobalTypes;

namespace Engine.Modules
{
    public partial class Collider
    {
        [Init(nameof(Init))]
        public static class Updater
        {
            private static readonly LockList<Collider> _allColliders = new();

            private static void Init() => FrameEvents.EndUpdate.Add(Update, EndUpdateOrders.ColliderUpdater);

            public static void Register(Collider collider) => _allColliders.Add(collider);
            public static void Unregister(Collider collider) => _allColliders.Remove(collider);

            private static void Update()
            {
                _allColliders.LockRun(UpdateShapes);

                _allColliders.LockRun(CheckIntersections);
            }
            private static void CheckIntersections() => _allColliders.PForEach(c => c.CheckIntersections(_allColliders));
            private static void UpdateShapes() => _allColliders.PForEach(c => c.UpdateShape());
        }

    }
}