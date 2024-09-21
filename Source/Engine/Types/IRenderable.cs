using Microsoft.Xna.Framework;

namespace Engine.Types
{
    public interface IRenderable
    {
        public Drawing.IDrawer Drawer { get; }
        public bool CanDraw { get; set; }
        public void Draw(GameTime gameTime);
    }
}
