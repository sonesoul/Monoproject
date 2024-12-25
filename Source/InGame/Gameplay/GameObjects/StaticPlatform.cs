using Engine.Modules;
using Engine.Types;
using Engine;
using InGame.Interfaces;
using Engine.Drawing;
using GlobalTypes.Interfaces;

namespace InGame.GameObjects
{
    public class StaticPlatform : ILevelObject
    {
        public bool IsDestroyed { get; set; } = false;
        public Vector2 Position { get; private set; }

        public StringObject Object { get; private set; }

        public StaticPlatform(Vector2 position)
        {
            Position = position;

            Rigidbody rigidbody = new()
            {
                BodyType = BodyType.Static
            };
            Collider collider = new()
            {
                Shape = Polygon.Rectangle(Level.TileSize)
            };

            Object = new("", Fonts.Silk, true, collider, rigidbody)
            {
                Position = this.Position
            };

            collider.IsShapeVisible = false;

            Drawer.Register(Draw, layer: 0);
        }

        private void Draw(DrawContext context)
        {
            var tileSize = Level.TileSize;

            Rectangle rect = new((Position - tileSize / 2).ToPoint(), Level.TileSize.ToPoint());

            context.Rectangle(rect, Palette.White);
        }

        public void Destroy() => IDestroyable.Destroy(this); 
        public void ForceDestroy() 
        {
            Object?.ForceDestroy();
            Object = null;

            Drawer.Unregister(Draw);
        }
    }
}