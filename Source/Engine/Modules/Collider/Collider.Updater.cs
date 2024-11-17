using GlobalTypes.Events;
using GlobalTypes;
using System.Collections.Generic;

namespace Engine.Modules
{
    public partial class Collider
    {
        public static class Updater
        {
            private static readonly List<Collider> _allColliders = new();
            private static readonly List<Collider> _toRemove = new();
            private static readonly List<Collider> _toAdd = new();

            private static bool isIterating = false;

            [Init]
            private static void Init() => FrameEvents.EndUpdate.Add(Update, EndUpdateOrders.ColliderUpdater);

            public static void Register(Collider collider)
            {
                if (!isIterating)
                {
                    _allColliders.Add(collider);
                }
                else
                {
                    _toAdd.Add(collider);
                }
            }
            public static void Unregister(Collider collider)
            {
                if (!isIterating) 
                {
                    _allColliders.Remove(collider);
                }
                else
                {
                    _toRemove.Add(collider);
                }
            }

            private static void Update()
            {
                isIterating = true;
                UpdateShapes();
                CheckIntersections();
                isIterating = false;

                if (_toAdd.Count > 0)
                {
                    _allColliders.AddRange(_toAdd);
                    _toAdd.Clear();
                }

                if (_toRemove.Count > 0)
                {
                    _toRemove.ForEach(c => _allColliders.Remove(c));
                    _toRemove.Clear();
                }
            }
            private static void CheckIntersections()
            {
                _allColliders.PForEach(c =>
                {
                    if (c == null)
                    {
                        Unregister(c);
                        return;
                    }

                    c.CheckIntersections(_allColliders);
                });
            }
            private static void UpdateShapes()
            {
                _allColliders.PForEach(c =>
                {
                    if (c == null)
                    {
                        Unregister(c);
                        return;
                    }

                    c.UpdateShape();
                });
            }
        }

    }
}