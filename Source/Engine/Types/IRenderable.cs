using Microsoft.Xna.Framework;

namespace Engine.Types
{
    public interface IRenderable
    {
        public Drawing.IDrawer Drawer { get; }
        public void Draw(GameTime gameTime);
    }
}
