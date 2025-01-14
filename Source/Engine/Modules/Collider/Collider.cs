﻿using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Types;
using Engine.Drawing;

namespace Engine.Modules
{
    public partial class Collider : ObjectModule
    {
        public Polygon Shape { get => shape; set => shape = value; } 
        public Rectangle Bounds => bounds;
        public IReadOnlyList<Collider> Intersections => collisions;
        public bool Intersects => collisions.Count > 0;

        public bool IsShapeVisible { get; set; } = true;
        public Color BaseColor { get; set; } = Palette.White;
        public Color IntersectColor { get; set; } = Palette.White;

        public event Action<Collider> ColliderEnter, ColliderStay, ColliderExit;
        
        private List<Collider> collisions = new();
        private List<Collider> previousCollisions = new();

        private Rectangle bounds;
        private Polygon shape = Polygon.Rectangle(50, 50);
        
        private List<Vector2> Vertices => Shape.Vertices;

        public Collider(ModularObject owner = null) : base(owner) { }
        protected override void PostConstruct()
        {
            Drawer.Register(DrawShape, true);
           
            Updater.Register(this);
        }
        
        public bool IntersectsWith(Collider other)
        {
            if (other == null || other.IsDestroyed || other.Owner.IsDestroyed)
                return false;

            return Shape.IntersectsWith(other.Shape);
        }
        public bool IntersectsWith(Collider other, out Vector2 mtv)
        {
            mtv = Vector2.Zero;

            if (other == null || other.IsDestroyed || other.Owner.IsDestroyed)
                return false;

            return Shape.IntersectsWith(other.Shape, out mtv);
        }
        public Vector2 GetMTV(Collider other) => Shape.GetMTV(other.Shape);

        public bool ContainsPoint(Vector2 point) => Shape.ContainsPoint(point);

        private void DrawShape(DrawContext context)
        {
            UpdateShape();
            if (IsShapeVisible)   
                context.HollowPoly(Shape.WorldVertices, (collisions.Count > 0) ? IntersectColor : BaseColor, 1);
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

            foreach (var item in colliders.Where(c => IsInProximity(c, 2) && c != this && !IsDestroyed))
            {
                if (IntersectsWith(item, out var mtv))
                {
                    collisions.Add(item);

                    if (!previousCollisions.Contains(item))
                        ColliderEnter?.Invoke(item);
                    else
                        ColliderStay?.Invoke(item);
                }
            }

            foreach (var item in previousCollisions)
            {
                if (!Intersections.Contains(item))
                    ColliderExit?.Invoke(item);
            }

            previousCollisions.Clear();
            previousCollisions.AddRange(collisions);
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

        public override void ForceDestroy()
        {
            base.ForceDestroy();

            Updater.Unregister(this);
            Drawer.Unregister(DrawShape);
            
            collisions?.Clear();
            previousCollisions?.Clear();

            collisions = null;
            previousCollisions = null;
        }
    }
}