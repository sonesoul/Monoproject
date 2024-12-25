using Engine.Drawing;
using GlobalTypes;
using GlobalTypes.Interfaces;
using InGame.Interfaces;
using InGame.Managers;
using Monoproject;
using System;
using System.Collections;

namespace InGame.Overlays.Screens
{
    public class GameOverScreen : IOverlayScreen
    {
        private Vector2 scale;

        private Vector2 baseScale = Window.Size * 1.5f;
        private Vector2 finalScale = new(Window.Width * 0.7f, Window.Height * 0.9f);

        private float alpha = 0;
        private float finalAlpha = 180;

        private StepTask animationTask;

        private HotKeyButton restartButton, exitButton;
        private bool isTextVisible;

        public void Show()
        {
            Drawer.Register(Draw, false);
            scale = baseScale;

            restartButton = CreateButton("Restart", Window.Center.WhereY(y => y * 1.7f), Session.Restart);
            exitButton = CreateButton("Quit the game", restartButton.Position.WhereY(y => y + restartButton.Size.Y * 1.5f), Main.Instance.Exit);

            animationTask = StepTask.Run(ShowGameOver(), false);
        }
        public void Hide()
        {
            animationTask?.Dispose();
            alpha = 0;
            scale = baseScale;

            restartButton.Destroy();
            exitButton.Destroy();
            
            Drawer.Unregister(Draw);
        }

        public void Enable()
        {
            restartButton.Enabled = true;
            exitButton.Enabled = true;
        }
        public void Disable()
        {
            restartButton.Enabled = false;
            exitButton.Enabled = false;
        }

        private void Draw(DrawContext context)
        {
            var rect = new Rectangle(Window.Center.ToPoint(), scale.ToPoint());

            Color color = new(Palette.Black, (byte)alpha);
            rect.Location -= (scale / 2).ToPoint();

            context.Rectangle(rect, color);

            Rectangle bounds = new(rect.Location, rect.Size);
            context.HollowRect(bounds, Palette.White, 1);

            if (!isTextVisible)
                return;

            var font = Fonts.SilkBold;
            string gameOver = "Game over!";
            context.String(
                font, 
                gameOver, 
                Window.Center.WhereY(y => y - rect.Size.Y / 3), 
                Palette.White, 
                font.MeasureString(gameOver) / 2, 
                new(1.5f));
        }

        private IEnumerator ShowGameOver()
        {
            yield return StepTask.DelayUnscaled(1);
            
            alpha = finalAlpha;
            scale = finalScale;
            isTextVisible = true;

            yield return StepTask.DelayUnscaled(1);
            restartButton.Color = new(restartButton.Color, 255);

            yield return StepTask.DelayUnscaled(0.3f);
            exitButton.Color = new(exitButton.Color, 255);
            
            restartButton.Enabled = true;
            exitButton.Enabled = true;
        }

        private HotKeyButton CreateButton(string text, Vector2 position, Action pressAction)
        {
            HotKeyButton button = new(text)
            {
                Position = position,
                Color = new(Palette.White, 0),
                IsTimeScaled = false,
                Enabled = false,
            };

            button.HotKeyReleased += () =>
            {
                pressAction();
                Disable();
            };

            return button;
        }
    }
}