namespace Engine.Types.Interfaces
{
    public interface IRenderable
    {
        public Drawing.IDrawer Drawer { get; }
        public bool CanDraw { get; set; }
        public void Draw();
    }
}