using Engine.Drawing;
using GlobalTypes;
using GlobalTypes.Interfaces;
using InGame.Interfaces;
using InGame.Visuals;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;

namespace InGame.Overlays
{
    public class InfoWindow : VisualElement, IDestroyable, IOverlayScreen
    {
        public bool IsDestroyed { get; set; } = false;

        public SpriteFont Font { get; set; } = Fonts.Silk;
        public bool IsVisible { get; set; } = true;

        public string Text { get; set; }

        private StepTask currentTask = null;

        public InfoWindow(string text)
        {
            Text = text;
            Position = Window.Center.WhereY(y => y - 90);
        }

        private void Draw(DrawContext context)
        {
            if (!IsVisible) 
                return;

            Vector2 size = Font.MeasureString(Text);
            Vector2 origin = size / 2;

            size = (size * Scale).Both() + 30;
            Rectangle rect = new((Position - size / 2).ToPoint(), size.ToPoint());

            context.Rectangle(rect, new(Palette.Black, (byte)(Alpha / 2)));
            context.String(Font, Text, Position, new(Palette.White, (byte)Alpha), origin, Scale);
        }

        public void Destroy() => IDestroyable.Destroy(this); 
        public void ForceDestroy()
        {
            Drawer.Unregister(Draw);
        }

        public void Show()
        {
            Drawer.Register(Draw, false);
            StepTask.Replace(ref currentTask, ShowLerp);
        }
        public void Hide()
        {
            StepTask.Replace(ref currentTask, HideLerp);
            currentTask.Completed += (t) => Drawer.Unregister(Draw);
        }

        public void Enable()
        {
            
        }
        public void Disable()
        {
            
        }

        private IEnumerator ShowLerp()
        {
            Vector2 finalPosition = Position.WhereY(y => y - 10);

            yield return StepTask.Interpolate((ref float e) =>
            {
                Alpha = MathHelper.Lerp(Alpha, 255, e);
                Position = Vector2.Lerp(Position, finalPosition, e);

                e += FrameState.DeltaTimeUnscaled / 0.2f;
            });
        }
        private IEnumerator HideLerp()
        {
            yield return StepTask.Interpolate((ref float e) =>
            {
                Alpha = MathHelper.Lerp(Alpha, 0, e);

                e += FrameState.DeltaTime;
            });
        }
    }
}