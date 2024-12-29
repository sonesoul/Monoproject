using Engine.Drawing;
using GlobalTypes;
using GlobalTypes.Extensions;
using GlobalTypes.Interfaces;
using InGame.Interfaces;
using InGame.Managers;
using InGame.Visuals;
using Monoproject;
using System;
using System.Collections;

namespace InGame.Overlays.Screens
{
    public class GameOverScreen : IOverlayScreen
    {
        private class ScoreElement : VisualElement
        {
            public float Value { get; set; } = 0;

            private StepTask scrollTask;
            public void SetScore(float value)
            {
                StepTask.Replace(ref scrollTask, ScrollValue(value), false);
            }

            public IEnumerator ScrollValue(float end)
            {
                yield return StepTask.Interpolate((ref float e) =>
                {
                    Value = MathHelper.Lerp(Value, end, e);
                    e += FrameState.DeltaTimeUnscaled / 2;
                });
            }
        }

        private Vector2 rectScale = new(Window.Width * 0.5f + 2, Window.Height * 0.7f);

        private Vector2 screenPosition = Window.Center;
        private float rectAlpha = 180;
        
        private StepTask animationTask;
        private ScoreElement scoreVisual = new();
        private HotKeyButton restartButton, exitButton;

        private bool rectVisible, scoreVisible, gameOverVisible;

        public void Show()
        {
            Drawer.Register(Draw, false);
           
            restartButton = CreateButton("Restart", screenPosition.WhereY(y => y + rectScale.Y / 3f), Session.Restart);
            exitButton = CreateButton("Quit the game", restartButton.Position.WhereY(y => y + restartButton.Size.Y * 1.5f), Main.Instance.Exit);

            animationTask = StepTask.Run(ShowGameOver(), false);
        }
        public void Hide()
        {
            animationTask?.Dispose();
            rectAlpha = 0;
            
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
            SetButtonPositions();

            if (rectVisible)
                DrawRect(context);

            if (scoreVisible)
                DrawScore(context);

            if (gameOverVisible) 
                DrawGameOver(context);
        }

        private void DrawRect(DrawContext context)
        {
            var rect = new Rectangle(screenPosition.ToPoint(), rectScale.ToPoint());

            Color color = new(Palette.Black, (byte)rectAlpha);
            rect.Location -= (rectScale / 2).ToPoint();

            context.Rectangle(rect, color);

            Rectangle bounds = new(rect.Location, rect.Size);
            context.HollowRect(bounds, Palette.White, 1);
        }
        private void DrawScore(DrawContext context)
        {
            var font = Fonts.Silk;

            string scoreStr = "";
            Vector2 scoreStrSize = font.MeasureString(scoreStr);

            Vector2 offset = new(0, -30);

            context.String(
                font,
                scoreStr, 
                screenPosition.WhereY(y => y - scoreStrSize.Y) + offset, 
                Palette.White, 
                scoreStrSize / 2, 
                new(1.5f));

            string scoreValue = $"{(int)scoreVisual.Value}";
            Vector2 scoreSize = font.MeasureString(scoreValue);

            context.String(
                font,
                scoreValue,
                screenPosition + new Vector2(0, 10) + offset,
                scoreVisual.ScaledColor,
                scoreSize / 2,
                new Vector2(1.5f) * scoreVisual.Scale);
        }
        private void DrawGameOver(DrawContext context)
        {
            var font = Fonts.SilkBold;
            string gameOver = "Game over";
            context.String(
                font,
                gameOver,
                screenPosition.WhereY(y => y - rectScale.Y / 2.5f),
                Palette.White,
                font.MeasureString(gameOver) / 2,
                new(1.5f));
        }
        
        private void SetButtonPositions()
        {
            restartButton.Position = screenPosition.WhereY(y => y + rectScale.Y / 3f);
            exitButton.Position = restartButton.Position.WhereY(y => y + restartButton.Size.Y * 1.5f);
        }

        private IEnumerator ShowGameOver()
        {
            yield return StepTask.DelayUnscaled(1);
            rectVisible = true;
            gameOverVisible = true;
            rectAlpha = 255;

            yield return StepTask.DelayUnscaled(1);

            scoreVisual.Alpha = 255;
            scoreVisible = true;
            scoreVisual.Value = SessionManager.Score.GetTotal();


            Vector2 radius = new(10, 10);
            Random rnd = new();

            yield return ShakeScreen((-radius).Randomize(radius, rnd), 0.8f);
            restartButton.Color = new(restartButton.Color, 255);
            exitButton.Color = new(exitButton.Color, 255);
            
            restartButton.Enabled = true;
            exitButton.Enabled = true;

            yield return ShakeScreen((-radius).Randomize(radius, rnd), 3f);
        }
        
        private IEnumerator ShakeScreen(Vector2 position, float time)
        {
            screenPosition += position / 2;
            yield return null;
            screenPosition += position / 2;
            yield return StepTask.Interpolate((ref float e) =>
            {
                screenPosition = Vector2.Lerp(screenPosition, Window.Center, e);

                e += FrameState.DeltaTimeUnscaled / time;
            });
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