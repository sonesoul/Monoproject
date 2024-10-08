using GlobalTypes.Collections;
using GlobalTypes.Events;
using GlobalTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Engine.Types;
using Microsoft.Xna.Framework;
using Engine.Drawing;

namespace Engine.Modules
{
    public class Collider : ObjectModule
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

        public Polygon Shape { get => shape; set => shape = value; } 
        public Rectangle Bounds => bounds;
        public IReadOnlyList<Collider> Intersections => collisions;
        public bool Intersects => collisions.Count > 0;

        public event Action<Collider> OnOverlapEnter, OnOverlapStay, OnOverlapExit;
        
        private readonly List<Collider> collisions = new();
        private readonly List<Collider> previousCollisions = new();

        private Rectangle bounds;
        private Polygon shape = Polygon.Rectangle(50, 50);
        private Color shapeDrawColor = Color.Lime;

        private ShapeDrawer shapeDrawer;
        
        private List<Vector2> Vertices => Shape.Vertices;

        public Collider(ModularObject owner = null) : base(owner) { }
        protected override void PostConstruct()
        {
            shapeDrawer = new(InstanceInfo.GraphicsDevice, InstanceInfo.SpriteBatch);
            IngameDrawer.Instance.AddDrawAction(DrawShape, DrawBounds);
            Updater.Register(this);
        }
        
        public bool IntersectsWith(Collider other)
        {
            if (other == null || other.IsDisposed || other.Owner.IsDestroyed)
                return false;

            return Shape.IntersectsWith(other.Shape);
        }
        public bool IntersectsWith(Collider other, out Vector2 mtv)
        {
            mtv = Vector2.Zero;

            if (other == null || other.IsDisposed || other.Owner.IsDestroyed)
                return false;

            return Shape.IntersectsWith(other.Shape, out mtv);
        }
        public Vector2 GetMTV(Collider other) => Shape.GetMTV(other.Shape);

        public bool IsPointWithin(Vector2 point) => Shape.IsPointWithin(point);

        private void DrawShape()
        {
            UpdateShape();
            
            Vector2 current = Vertices[0] + Owner.IntegerPosition;
            Vector2 next;
            for (int i = 1; i < Vertices.Count; i++)
            {
                next = Vertices[i] + Owner.IntegerPosition;
                shapeDrawer.DrawLine(current, next, shapeDrawColor);
                current = next;
            }


            shapeDrawer.DrawLine(current, Vertices[0] + Owner.IntegerPosition, shapeDrawColor);
            
        }
        private void DrawBounds()
        {
            //shapeDrawer.DrawRectangle(Bounds, Color.Gray);
        }

        public void UpdateShape()
        {
            shape.position = Owner.IntegerPosition;
            shape.Rotation = Owner.RotationDeg;
            
            UpdateBounds();
        }
        private void UpdateBounds()
        {
            var verts = Vertices;

            int width = (int)verts.Max(v => v.X).Ceiled() - (int)verts.Min(v => v.X).Floored();
            int height = (int)verts.Max(v => v.Y).Ceiled() - (int)verts.Min(v => v.Y).Floored();

            bounds.X = (int)(Owner.IntegerPosition.X - width / 2) - 1;
            bounds.Y = (int)(Owner.IntegerPosition.Y - height / 2) - 1;

            bounds.Width = width + 1;
            bounds.Height = height + 1;
        }

        private void CheckIntersections(IEnumerable<Collider> colliders)
        {
            collisions.Clear();

            foreach (var item in colliders.Where(c => IsInProximity(c, 2) && c != this && !IsDisposed))
            {
                if (IntersectsWith(item, out var mtv))
                {
                    collisions.Add(item);

                    if (!previousCollisions.Contains(item))
                        OnOverlapEnter?.Invoke(item);
                    else
                        OnOverlapStay?.Invoke(item);
                }
            }

            foreach (var item in previousCollisions)
            {
                if (!Intersections.Contains(item))
                    OnOverlapExit?.Invoke(item);
            }

            previousCollisions.Clear();
            previousCollisions.AddRange(collisions);

            if (collisions.Count > 0)
            {
                shapeDrawColor = Color.Red;
            }
            else
            {
                shapeDrawColor = Color.LightGreen;
            }
        }
        
        private bool IsInProximity(Collider other, float distance)
        {
            Rectangle thisBounding = Bounds;
            Rectangle otherBounding = other.Bounds;

            float dx = Math.Max(thisBounding.Left, otherBounding.Left) - Math.Min(thisBounding.Right, otherBounding.Right);
            float dy = Math.Max(thisBounding.Top, otherBounding.Top) - Math.Min(thisBounding.Bottom, otherBounding.Bottom);

            Vector2 max = new(Math.Max(0, dx), Math.Max(0, dy));

            return max.LengthSquared() <= distance * distance;
        }

        protected override void PostDispose()
        {
            Updater.Unregister(this);
            
            IngameDrawer.Instance.RemoveDrawAction(DrawBounds);
            IngameDrawer.Instance.RemoveDrawAction(DrawShape);
            
            collisions.Clear();
            previousCollisions.Clear();
        }
    }
}