using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GlobalTypes;
using System;

namespace Engine.Drawing
{
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