using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Drawing;
using Engine.Types;
using GlobalTypes.Events;
using GlobalTypes.Interfaces;
using GlobalTypes.Collections;
using System.Threading.Tasks;

namespace Engine.Modules
{
    public class Collider : ObjectModule
    {
        public class CollisionManager : IInitable
        {
            //colliders must be updated after the rigidbody is updated
            public static int UpdateOrder => Rigidbody.UpdateOrder + 1;

            private static readonly LockList<Collider> _allColliders = new();
            void IInitable.Init() => GameEvents.EndUpdate.Insert(Update, UpdateOrder);

            public static void Register(Collider collider) => _allColliders.Add(collider);
            public static void Unregister(Collider collider) => _allColliders.Remove(collider);

            public static void Update(GameTime gt)
            {
                _allColliders.LockRun(
                    () => Parallel.ForEach(_allColliders, 
                    c => c?.CheckIntersections(_allColliders)));
            }
        }


        public Polygon polygon;
        public Color drawColor = Color.Green;

        #region Events
        public event Action<Collider> IntersectionEnter;
        public event Action<Collider> IntersectionStay;
        public event Action<Collider> IntersectionExit;

        public event Action<Collider> TriggerEnter;
        public event Action<Collider> TriggerStay;
        public event Action<Collider> TriggerExit;
        #endregion

        private readonly List<Collider> _intersections = new();
        private readonly List<Collider> _previousIntersections = new();

        private Rectangle _bounding;
        private List<Vector2> PolygonVerts => polygon.Vertices;

        private ColliderMode _colliderMode;
        
        private Action<IReadOnlyList<Collider>> _intersectionChecker;
        private readonly Action<GameTime> _drawAction;
        
        private readonly ShapeDrawer _shapeDrawer;
        
        public Rectangle ShapeBounding => _bounding;
        public bool Intersects { get; private set; }
        public ColliderMode Mode { get => _colliderMode; set => SetUpdater(value); }
        public IReadOnlyList<Collider> Intersections => _intersections;

        public Collider(ModularObject owner) : base(owner)
        {
            _drawAction = Draw;
            
            polygon = new(owner?.IntegerPosition ?? Vector2.Zero, Polygon.RectangleVerts(50, 50));
            _shapeDrawer = new(IngameDrawer.Instance.GraphicsDevice, IngameDrawer.Instance.SpriteBatch);

            IngameDrawer.Instance.AddDrawAction(_drawAction);
            CollisionManager.Register(this);

            UpdateBounding();
            SetUpdater(ColliderMode.Physical);
        }


        public void CheckIntersections(IReadOnlyList<Collider> colliders)
        {
            _intersections.Clear();

            UpdatePolygon();

            _intersectionChecker(colliders);

            if (Owner == null)
                return;

            AfterCheck();
        }
        private void UpdatePolygon()
        {
            polygon.position = Owner.IntegerPosition;
            polygon.Rotation = Owner.Rotation;
            UpdateBounding();
        }
        private void AfterCheck()
        {
            _previousIntersections.Clear();
            _previousIntersections.AddRange(_intersections);
            Intersects = _intersections.Any();
            drawColor = Intersects ? Color.Red : Color.Green;

            UpdatePolygon();
        }

        private void PhysicalCheck(IReadOnlyList<Collider> colliders)
        {
            foreach (var item in colliders.Where(c => (c.Mode == ColliderMode.Physical || c.Mode == ColliderMode.Static) && IsWithinDistance(c, 1)))
            {
                if (item == this)
                    continue;
               
                if (polygon.IntersectsWith(item.polygon, out var mtv))
                {
                    _intersections.Add(item);

                    //bruh idk why the fuck it works better than Round or Floor methods
                    //upd: ok its just an int casting, but I won't remove it
                    mtv = mtv.ToPoint().ToVector2();

                    PushOut(item, mtv);

                    if (!_previousIntersections.Contains(item))
                        IntersectionEnter?.Invoke(item);
                    else
                        IntersectionStay?.Invoke(item);

                    UpdatePolygon();
                }
                else if (_previousIntersections.Contains(item))
                    IntersectionExit?.Invoke(item);
            }
        }
        private void StatiCheck(IReadOnlyList<Collider> colliders)
        {
            foreach (var item in colliders.Where(c => (c.Mode == ColliderMode.Physical) && IsWithinDistance(c, 1)))
            {
                if (item == this)
                    continue;

                if (polygon.IntersectsWith(item.polygon, out var mtv))
                {
                    _intersections.Add(item);

                    if (!_previousIntersections.Contains(item))
                        IntersectionEnter?.Invoke(item);
                    else
                        IntersectionStay?.Invoke(item);
                }
                else if (_previousIntersections.Contains(item))
                    IntersectionExit?.Invoke(item);
            }
        }
        private void TriggerCheck(IReadOnlyList<Collider> colliders)
        {
            foreach (var item in colliders.Where(c => IsWithinDistance(c, 1)))
            {
                if (item == this)
                    continue;

                if (polygon.IntersectsWith(item.polygon))
                {
                    _intersections.Add(item);

                    if (!_previousIntersections.Contains(item))
                        TriggerEnter?.Invoke(item);
                    else
                        TriggerStay?.Invoke(item);
                }
                else if (_previousIntersections.Contains(item))
                    TriggerExit?.Invoke(item);
            }
        }

        //physical-physical touches will be fixed later
        //the problem is that each object tries to push out others. Fix: there should be correct priorites for push orders. Must also handle parallel calls.
        public void PushOut(Collider other, Vector2 mtv)
        {
            Vector2 dir = Owner.IntegerPosition - other.Owner.IntegerPosition;
            if (Vector2.Dot(dir, mtv) < 0)
                mtv = -mtv;

            if (other.Mode == ColliderMode.Physical)
                other.Owner.position -= mtv;
            else
                Owner.position += mtv;
        }

        private void UpdateBounding()
        {
            var verts = PolygonVerts;

            int width = (int)verts.Max(v => v.X).Ceiled() - (int)verts.Min(v => v.X).Floored();
            int height = (int)verts.Max(v => v.Y).Ceiled() - (int)verts.Min(v => v.Y).Floored();

            _bounding.X = (int)Owner.position.X - width / 2;
            _bounding.Y = (int)Owner.position.Y - height / 2;
            _bounding.Width = width;
            _bounding.Height = height;
        }
        private bool IsWithinDistance(Collider other, float distance)
        {
            Rectangle myBounding = ShapeBounding;
            Rectangle otherBounding = other.ShapeBounding;

            float dx = Math.Max(myBounding.Left, otherBounding.Left) - Math.Min(myBounding.Right, otherBounding.Right);
            float dy = Math.Max(myBounding.Top, otherBounding.Top) - Math.Min(myBounding.Bottom, otherBounding.Bottom);

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
        private void Draw(GameTime gt)
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
        protected override void Destruct()
        {
            IngameDrawer.Instance.RemoveDrawAction(_drawAction);
            CollisionManager.Unregister(this);

            IntersectionEnter = null;
            IntersectionStay = null;
            IntersectionExit = null;
            TriggerEnter = null;
            TriggerStay = null;
            TriggerExit = null;
        }
    }
}