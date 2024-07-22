using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Engine.FrameDrawing;
using Engine.Types;
using GlobalTypes.Events;

namespace Engine.Modules
{
    public class Collider : ObjectModule
    {
        public Polygon polygon;
        public Color drawColor = Color.Green;

        public event Action<Collider> OnTouchEnter;
        public event Action<Collider> OnTouchStay;
        public event Action<Collider> OnTouchExit;

        public event Action<Collider> OnTriggerEnter;
        public event Action<Collider> OnTriggerStay;
        public event Action<Collider> OnTriggerExit;
        public event Action<GameTime> OnCheckFinish;

        private readonly List<Collider> _intersections = new();
        private List<Collider> _lastFrameIntersections = new();

        private ColliderMode _colliderMode;
        private Action _intersectionUpdater;
        private readonly Action<GameTime> _drawAction;
        private readonly ShapeDrawer _shapeDrawer;
        private readonly static List<Collider> _allColliders = new();

        public ColliderMode Mode { get => _colliderMode; set => SetUpdater(value); }
        public string Info { get; set; } = "";
        public Rectangle ShapeBounding { get; private set; }
        public bool Intersects { get; private set; }
        public static IReadOnlyList<Collider> AllColliders => _allColliders;
        public IReadOnlyList<Collider> Intersections => _intersections;
        public Collider(GameObject owner) : base(owner)
        {
            Owner = owner;
            _drawAction = Draw;

            polygon = new(owner.position, Polygon.RectangleVerts(50, 50));
            _shapeDrawer = new(IngameDrawer.Instance.GraphicsDevice, IngameDrawer.Instance.SpriteBatch);

            IngameDrawer.Instance.AddDrawAction(_drawAction);

            ShapeBounding = GetShapeBounding();

            _allColliders.Add(this);

            GameEvents.OnUpdate.AddListener(Update);

            SetUpdater(ColliderMode.Physical);
        }

        private void Update(GameTime gt)
        {
            _intersections.Clear();
            polygon.position = Owner.position;
            polygon.Rotation = Owner.rotation;
            ShapeBounding = GetShapeBounding();

            _intersectionUpdater();
            OnCheckFinish?.Invoke(gt);

            _lastFrameIntersections = new List<Collider>(_intersections);

            Intersects = _intersections.Any();
            drawColor = Intersects ? Color.Red : Color.Green;
        }

        private void PhysicalCheck()
        {
            foreach (var item in AllColliders.Where(c => c.Mode == ColliderMode.Physical || c.Mode == ColliderMode.Static))
            {
                if (item == this)
                    continue;

                TextObject otherObj = item.Owner as TextObject;

                if (polygon.IntersectsWith(item.polygon, out var mtv))
                {
                    _intersections.Add(item);

                    PushOut(item, mtv);

                    if (!_lastFrameIntersections.Contains(item))
                        OnTouchEnter?.Invoke(item);
                    else
                        OnTouchStay?.Invoke(item);
                }
                else if (_lastFrameIntersections.Contains(item))
                    OnTouchExit?.Invoke(item);
            }
        }
        private void StatiCheck()
        {
            foreach (var item in AllColliders.Where(c => c.Mode == ColliderMode.Physical))
            {
                if (item == this)
                    continue;

                TextObject otherObj = item.Owner as TextObject;

                if (polygon.IntersectsWith(item.polygon))
                {
                    _intersections.Add(item);

                    if (!_lastFrameIntersections.Contains(item))
                        OnTouchEnter?.Invoke(item);
                    else
                        OnTouchStay?.Invoke(item);
                }
                else if (_lastFrameIntersections.Contains(item))
                    OnTouchExit?.Invoke(item);
            }
        }
        private void TriggerCheck()
        {
            foreach (var item in AllColliders)
            {
                if (item == this)
                    continue;

                TextObject otherObj = item.Owner as TextObject;

                if (polygon.IntersectsWith(item.polygon))
                {
                    _intersections.Add(item);

                    if (!_lastFrameIntersections.Contains(item))
                        OnTriggerExit?.Invoke(item);
                    else
                        OnTriggerExit?.Invoke(item);
                }
                else if (_lastFrameIntersections.Contains(item))
                    OnTriggerExit?.Invoke(item);
            }
        }

        public void PushOut(Collider other, Vector2 mtv)
        {
            Vector2 direction = Owner.IntegerPosition - other.Owner.IntegerPosition;

            if (Vector2.Dot(direction, mtv) < 0)
                mtv = -mtv;

            Vector2 displacement;

            if (Math.Abs(mtv.X) > Math.Abs(mtv.Y))
                displacement = new Vector2(mtv.X, 0);
            else
                displacement = new Vector2(0, mtv.Y);


            if (other.Mode == ColliderMode.Physical)
                other.Owner.position -= displacement;
            else if (other.Mode == ColliderMode.Static)
                Owner.position += displacement;
        }

        public Rectangle GetShapeBounding()
        {
            List<Vector2> vertices = polygon.Vertices;

            int width = (int)Math.Ceiling(vertices.Max(v => v.X)) - (int)Math.Floor(vertices.Min(v => v.X));
            int height = (int)Math.Ceiling(vertices.Max(v => v.Y)) - (int)Math.Floor(vertices.Min(v => v.Y));

            return new Rectangle(
                (int)Owner.position.X - width / 2,
                (int)Owner.position.Y - height / 2,
                width,
                height);
        }

        private void SetUpdater(ColliderMode newMode)
        {
            _colliderMode = newMode;
            switch (_colliderMode)
            {
                case ColliderMode.Physical:
                    _intersectionUpdater = PhysicalCheck;
                    break;
                case ColliderMode.Trigger:
                    _intersectionUpdater = TriggerCheck;
                    break;
                case ColliderMode.Static:
                    _intersectionUpdater = StatiCheck;
                    break;
            }
        }
        protected override void Destruct()
        {
            _allColliders.Remove(this);
            GameEvents.OnUpdate.RemoveListener(new(Update, 0));
            IngameDrawer.Instance.RemoveDrawAction(_drawAction);
        }
        public void Draw(GameTime gt)
        {
            List<Vector2> vertices = polygon.Vertices;
            Vector2 current = vertices[0] + Owner.position;

            for (int i = 1; i < vertices.Count; i++)
            {
                Vector2 next = vertices[i] + Owner.position;
                _shapeDrawer.DrawLine(current, next, drawColor);
                current = next;
            }

            _shapeDrawer.DrawLine(current, vertices[0] + Owner.position, drawColor);
        }
    }
}