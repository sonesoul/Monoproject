using Engine;
using Engine.Drawing;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using System.Collections.Generic;

namespace InGame.Managers
{
    public static class BoundManager
    {
        private static List<StringObject> walls = new();
        [Init]
        private static void Init()
        {
            CreateWalls();

            Drawer.Register(Draw, false);
        }

        private static void Draw(DrawContext context)
        {
            context.HollowRect(new Rectangle(Point.Zero, Window.Size.ToPoint()), Palette.White);
        }
        private static void CreateWalls()
        {
            int width = Window.Width;
            int height = Window.Height;

            int thickness = 40;
            int offset = thickness / 2;

            offset -= 1;

            //top
            Polygon topShape = Polygon.Rectangle(width, thickness);
            CreateWall(topShape, new(width / 2, -offset));

            //left
            Polygon leftShape = Polygon.Rectangle(thickness, height);
            CreateWall(leftShape, new(-offset, height / 2));

            offset++;

            //bottom
            Polygon botShape = Polygon.Rectangle(width + 1, thickness);
            CreateWall(botShape, new(width / 2 - 1, height + offset));

            //right
            Polygon rightShape = Polygon.Rectangle(thickness, height + 1);
            CreateWall(rightShape, new(width + offset, height / 2));
        }
        private static void CreateWall(Polygon shape, Vector2 position)
        {
            Collider coll = new()
            {
                Shape = shape,
                IsShapeVisible = false
            };
            Rigidbody rb = new() { BodyType = BodyType.Static };

            StringObject obj = new("", Fonts.Silk, true, coll, rb)
            {
                Position = position
            };
            walls.Add(obj);
        }
    }
}