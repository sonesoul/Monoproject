using Engine.Drawing;
using GlobalTypes;
using InGame.GameObjects;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace InGame.Visuals.StorageVisuals
{
    public static class StorageVisual
    {
        private class RequirementElement : VisualElement
        {
            public string Letter { get; set; }

            private StepTask moveTask = null;

            public void SetLetter(string newReq)
            {
                Letter = newReq;
                Scale = new(0, 0);
                
                StepTask.Replace(ref moveTask, MoveBack);
            }

            private IEnumerator MoveBack()
            {
                yield return StepTask.Interpolate((ref float e) =>
                {
                    Scale = Vector2.Lerp(Scale, Vector2.One, e);

                    e += FrameState.DeltaTime;
                });
            }
        }
        private class ProgressElement : VisualElement
        {
            public float Progress { get; set; } = 0;
            
            private StepTask scrollTask = null;
            private StepTask alphaLerp = null;

            public void SetProgress(float fillValue)
            {
                StepTask.Replace(ref scrollTask, ScrollValue(fillValue));
            }

            public void Show()
            {
                StepTask.Replace(ref alphaLerp, LerpAlpha(255));
            }
            public void Hide()
            {
                StepTask.Replace(ref alphaLerp, LerpAlpha(0));
            }

            private IEnumerator LerpAlpha(float newAlpha)
            {
                yield return StepTask.Interpolate((ref float e) =>
                {
                    Alpha = MathHelper.Lerp(Alpha, newAlpha, e);

                    e += FrameState.DeltaTime / 0.1f;
                });
            }
            private IEnumerator ScrollValue(float newValue)
            {
                Show();

                yield return StepTask.Interpolate((ref float e) =>
                {
                    Progress = MathHelper.Lerp((float)Progress, newValue, e);

                    e += FrameState.DeltaTime / 2;
                });

                Progress = newValue;

                yield return StepTask.Delay(1);
                Hide();
            }
        }

        private static SpriteFont BracketsFont => Fonts.Silk;
        private static SpriteFont LetterFont => Fonts.SilkBold;

        private static Vector2 Position { get; set; } 
       
        private static CodeStorage Storage { get; set; } = null;

        private static RequirementElement reqVisual = new();
        private static ProgressElement progressDisplay = new();

        [Init]
        private static void Init()
        {
            Drawer.Register(Draw);
            Level.Created += () =>
            {
                Storage = Level.GetObject<CodeStorage>();

                Position = Storage.Position;

                Storage.RequirementChanged += reqVisual.SetLetter;
                reqVisual.Letter = Storage.Requirement;

                Storage.ProgressChanged += UpdateProgress;
                UpdateProgress();
            };
            Level.Cleared += () =>
            {
                Storage = null;
                progressDisplay.Progress = 0;
                progressDisplay.Hide();
            };

            Player.ShowUIRequested += progressDisplay.Show;
            Player.HideUIRequested += progressDisplay.Hide;
        }

        private static void Draw(DrawContext context)
        {
            if (Storage == null)
                return;
            
            Vector2 offset = (new Vector2(15, 0) * reqVisual.Scale).WhereX(x => x.ClampMin(5));

            DrawOptions options = new()
            {
                origin = BracketsFont.MeasureString("[") / 2,
                font = BracketsFont,
                scale = new(1.4f, 2)
            };

            context.String(
                LetterFont,
                reqVisual.Letter,
                Position + reqVisual.Position,
                Palette.White,
                LetterFont.MeasureString(reqVisual.Letter) / 2, 
                reqVisual.Scale);

            options.position = Position - offset;
            context.String("[", options);

            offset.X -= 2;
            options.position = Position + offset;
            context.String("]", options);


            string timerText = Storage.Timer.ToString();

            Vector2 origin = LetterFont.MeasureString(timerText) / 2;

            context.String(
                LetterFont,
                timerText,
                Position.WhereY(y => y - 40),
                Palette.White,
                origin,
                Vector2.One);

            string progressStr = $"{(int)progressDisplay.Progress}%";

            context.String(
                LetterFont,
                progressStr,
                Position.WhereY(y => y + 40) + progressDisplay.Position,
                progressDisplay.ScaledColor,
                Fonts.Silk.MeasureString(progressStr) / 2,
                new(1f));
        }

        private static void UpdateProgress()
        {
            float progress = 100 * (Storage.Progress / Storage.Capacity);
            progressDisplay.SetProgress(progress);
        }
    }
}