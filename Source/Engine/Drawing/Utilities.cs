using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GlobalTypes;
using System;

namespace Engine.Drawing
{
    [Obsolete]
    public class ShapeDrawer
    {
        private Texture2D pixel;
        private SpriteBatch spriteBatch;
        public ShapeDrawer(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            pixel = new Texture2D(device, 1, 1);
            pixel.SetData(new[] { Color.White });

            this.spriteBatch = spriteBatch;
        }
        public ShapeDrawer(IDrawer drawer)
        {
            pixel = new Texture2D(drawer.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            spriteBatch = drawer.SpriteBatch;
        }
        public ShapeDrawer()
        {
            pixel = new Texture2D(InstanceInfo.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            spriteBatch = InstanceInfo.SpriteBatch;
        }
        public void DrawRectangle(Rectangle rect, Color color)
        {
            Rectangle[] rects = new Rectangle[4];

            rects[0] = new(rect.Left, rect.Top, rect.Width, 1);
            rects[1] = new(rect.Left, rect.Bottom, rect.Width, 1);
            rects[2] = new(rect.Left, rect.Top, 1, rect.Height);
            rects[3] = new(rect.Right, rect.Top, 1, rect.Height);
            foreach (var item in rects)
            {
                spriteBatch.Draw(pixel, item, color);
            }
        }
        public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            spriteBatch.Draw(pixel, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), (int)thickness),
                             null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }

        public static void DrawLine(Vector2 start, Vector2 end, IDrawer drawer, Color color, float thickness = 1f)
        {
            var pixel = new Texture2D(drawer.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            Rectangle rect = new((int)start.X, (int)start.Y, (int)edge.Length(), (int)thickness);
            Vector2 origin = new(0, 0.5f);
            drawer.SpriteBatch.Draw(pixel, rect, null, color, angle, origin, SpriteEffects.None, 0);
        }
    }

    public class Ruler
    {
        public bool IsActive { get; private set; } = false;

        public Vector2 Start { get; set; }
        public Vector2 End { get; set; }
        public Color LineColor { get; set; } = Color.White;
        public float Thickness { get; set; } = 1;
        public float Distance => Start.DistanceTo(End);


        public Color InfoColor { get; set; } = Color.White;
        public Vector2 InfoPosition { get; set; } = InstanceInfo.WindowSize / 2;
        public float InfoSize { get; set; } = 1;

        public void Show()
        {
            if (!IsActive)
            {
                IsActive = true;
                Drawer.Register(Draw, false);
            }
        }
        public void Hide()
        {
            if (IsActive) 
            {
                IsActive = false;
                Drawer.Unregister(Draw);
            }
        }

        public void Draw(DrawContext context)
        {
            context.Line(Start, End, LineColor, Thickness);
            
            SpriteFont font = InGame.UI.Silk;

            string startText = $"({Start.X}, {Start.Y})";
            string endText = $"({End.X}, {End.Y})";
            string distanceText = $"{startText} - {endText}\nD: [{((int)Distance)}]";
            Vector2 dm = font.MeasureString(distanceText) * InfoSize;

            context.String(
                font,
                distanceText,
                InfoPosition - dm / 2,
                InfoColor,
                0,
                Vector2.Zero,
                Vector2.One * InfoSize);
        }
    }
}