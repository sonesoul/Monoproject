using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Drawing;
using Engine.Types;
using GlobalTypes;
using GlobalTypes.Events;
using GlobalTypes.Collections;
using System.Threading.Tasks;

namespace Engine.Modules
{
    [Obsolete]
    public class OLDCollider : ObjectModule
    {
        public static class CollisionManager
        {
            private static readonly LockList<OLDCollider> _allColliders = new();

            private static void Init() => FrameEvents.EndUpdate.Add(EndUpdate, EndUpdateOrders.Collider);

            public static void Register(OLDCollider collider) => _allColliders.Add(collider);
            public static void Unregister(OLDCollider collider) => _allColliders.Remove(collider);

            public static void EndUpdate()
            {
                _allColliders.LockRun(
                    () => Parallel.ForEach(
                        _allColliders,
                        (c) => c?.CheckIntersections(_allColliders)));
            }
        }

        public Rectangle ShapeBounding => _bounding;
        public bool Intersects { get; private set; }
        public ColliderMode Mode { get => _colliderMode; set => SetUpdater(value); }
        public IReadOnlyList<OLDCollider> Intersections => _intersections;

        public Polygon polygon = Polygon.Rectangle(50, 50);
        public Color drawColor = Color.Green;

        public event Action<OLDCollider> OverlapEnter, OverlapStay, OverlapExit;
        
        private readonly List<OLDCollider> _intersections = new();
        private readonly List<OLDCollider> _previousIntersections = new();

        private Rectangle _bounding;

        private ColliderMode _colliderMode = ColliderMode.Physical;
        private Action<IReadOnlyList<OLDCollider>> _intersectionChecker;

        private Action _drawAction;
        private ShapeDrawer _shapeDrawer;
        private List<Vector2> PolygonVerts => polygon.Vertices;

        public OLDCollider(ModularObject owner = null) : base(owner) { }

        protected override void PostConstruct()
        {
            _drawAction = Draw;

            _shapeDrawer = new();

            IngameDrawer.Instance.AddDrawAction(_drawAction);
            CollisionManager.Register(this);

            UpdateBounding();
            SetUpdater(_colliderMode);
        }

        public bool IntersectsWith(OLDCollider other)
        {
            if (other == null || other.IsDisposed || other.Owner.IsDestroyed) 
                return false;

            return polygon.IntersectsWith(other.polygon);
        }
        public bool IntersectsWith(OLDCollider other, out Vector2 mtv)
        {
            mtv = Vector2.Zero;

            if (other == null || other.IsDisposed || other.Owner.IsDestroyed)
                return false;
            
            return polygon.IntersectsWith(other.polygon, out mtv);
        }

        private void CheckIntersections(IReadOnlyList<OLDCollider> colliders)
        {
            _intersections.Clear();

            UpdatePolygon();

            _intersectionChecker(colliders);

            if (Owner == null)
                return;

            _previousIntersections.Clear();
            _previousIntersections.AddRange(_intersections);
            Intersects = _intersections.Any();
            drawColor = Intersects ? Color.Red : Color.Green;

            UpdatePolygon();
        }
        private void UpdatePolygon()
        {
            polygon.position = Owner.IntegerPosition;
            polygon.Rotation = Owner.RotationDeg;
            UpdateBounding();
        }
        private void UpdateBounding()
        {
            var verts = PolygonVerts;

            int width = (int)verts.Max(v => v.X).Ceiled() - (int)verts.Min(v => v.X).Floored();
            int height = (int)verts.Max(v => v.Y).Ceiled() - (int)verts.Min(v => v.Y).Floored();

            _bounding.X = (int)Owner.Position.X - width / 2;
            _bounding.Y = (int)Owner.Position.Y - height / 2;
            _bounding.Width = width;
            _bounding.Height = height;
        }

        private void Draw()
        {
            Vector2 current = PolygonVerts[0] + Owner.IntegerPosition;
            Vector2 next;
            for (int i = 1; i < PolygonVerts.Count; i++)
            {
                next = PolygonVerts[i] + Owner.IntegerPosition;
                _shapeDrawer.DrawLine(current, next, drawColor);
                current = next;
            }


            _shapeDrawer.DrawLine(current, PolygonVerts[0] + Owner.IntegerPosition, drawColor);
        }

        private void PhysicalCheck(IReadOnlyList<OLDCollider> colliders)
        {
            foreach (var item in colliders.Where(c => IsWithinDistance(c, 2)))
            {
                if (item == this || IsDisposed)
                    continue;
               
                if (polygon.IntersectsWith(item.polygon, out var mtv))
                {
                    _intersections.Add(item);

                    //bruh idk why the fuck it works better than Round or Floor methods
                    //upd: ok its just an int casting, but I won't remove it
                    mtv = mtv.ToPoint().ToVector2();

                    PushOut(item, mtv);

                    if (!_previousIntersections.Contains(item))
                        OverlapEnter?.Invoke(item);
                    else
                        OverlapStay?.Invoke(item);

                    UpdatePolygon();
                }
            }

            foreach (var item in _previousIntersections)
            {
                if (!Intersections.Contains(item))
                    OverlapExit?.Invoke(item);
            }
        }
        private void StatiCheck(IReadOnlyList<OLDCollider> colliders)
        {
            foreach (var item in colliders.Where(c => (c.Mode != ColliderMode.Static) && IsWithinDistance(c, 2)))
            {
                if (item == this || IsDisposed)
                    continue;

                if (polygon.IntersectsWith(item.polygon))
                {
                    _intersections.Add(item);

                    if (!_previousIntersections.Contains(item))
                        OverlapEnter?.Invoke(item);
                    else
                        OverlapStay?.Invoke(item);
                }
            }

            foreach (var item in _previousIntersections)
            {
                if (!Intersections.Contains(item))
                    OverlapExit?.Invoke(item);
            }
        }
        private void TriggerCheck(IReadOnlyList<OLDCollider> colliders)
        {
            foreach (var item in colliders.Where(c => IsWithinDistance(c, 2)))
            {
                if (item == this || IsDisposed)
                    continue;

                if (polygon.IntersectsWith(item.polygon))
                {
                    _intersections.Add(item);

                    if (!_previousIntersections.Contains(item))
                        OverlapEnter?.Invoke(item);
                    else
                        OverlapStay?.Invoke(item);
                }
            }

            foreach (var item in _previousIntersections)
            {
                if (!Intersections.Contains(item))
                    OverlapExit?.Invoke(item);
            }
        }

        //physical-physical touches will be fixed later
        //the problem is that each object tries to push out others. Fix: there should be correct priorites for push orders. Must also handle parallel calls.
        private void PushOut(OLDCollider other, Vector2 mtv)
        {
            Vector2 dir = Owner.IntegerPosition - other.Owner.IntegerPosition;
            if (Vector2.Dot(dir, mtv) < 0)
                mtv = -mtv;

            if (other.Mode == ColliderMode.Physical)
                other.Owner.Position -= mtv;
            else if (other.Mode == ColliderMode.Static)
                Owner.Position += mtv;
        }

        private bool IsWithinDistance(OLDCollider other, float distance)
        {
            Rectangle thisBounding = ShapeBounding;
            Rectangle otherBounding = other.ShapeBounding;

            float dx = Math.Max(thisBounding.Left, otherBounding.Left) - Math.Min(thisBounding.Right, otherBounding.Right);
            float dy = Math.Max(thisBounding.Top, otherBounding.Top) - Math.Min(thisBounding.Bottom, otherBounding.Bottom);

            float maxDx = Math.Max(0, dx);
            float maxDy = Math.Max(0, dy);

            return (maxDx * maxDx) + (maxDy * maxDy) <= distance * distance;
        }
       
        private void SetUpdater(ColliderMode newMode)
        {
            _colliderMode = newMode;
            switch (_colliderMode)
            {
                case ColliderMode.Physical:
                    _intersectionChecker = PhysicalCheck;
                    break;
                case ColliderMode.Trigger:
                    _intersectionChecker = TriggerCheck;
                    break;
                case ColliderMode.Static:
                    _intersectionChecker = StatiCheck;
                    break;
            }
        }

        protected override void PostDispose()
        {
            IngameDrawer.Instance.RemoveDrawAction(_drawAction);
            CollisionManager.Unregister(this);

            _intersections.Clear();
            _previousIntersections.Clear();

            OverlapEnter = null;
            OverlapStay = null;
            OverlapExit = null;
        }
    }
}