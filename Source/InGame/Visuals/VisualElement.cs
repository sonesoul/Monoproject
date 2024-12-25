namespace InGame.Visuals
{
    public abstract class VisualElement
    {
        public Vector2 Position { get; set; } = Vector2.Zero;
        public Vector2 Scale { get; set; } = Vector2.One;

        public float Alpha { get; set; }
        public Color DrawColor { get; set; } = Palette.White;
        public Color ScaledColor => new(DrawColor, (byte)Alpha);
    }
}