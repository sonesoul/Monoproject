using Engine.Drawing;
using GlobalTypes;
using InGame.GameObjects;
using Microsoft.Xna.Framework.Graphics;
using System;
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
                StepTask.Replace(ref moveTask, ScaleUp);
            }
            private IEnumerator ScaleUp()
            {
                reqVisual.Scale = new(0);
                reqVisual.Alpha = 255;

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

            public event Action ScrollEnded;
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

                float startProgress = Progress;

                yield return StepTask.Interpolate((ref float e) =>
                {
                    Progress = MathHelper.Lerp(Progress, newValue, e);

                    e += FrameState.DeltaTime / 1.3f;
                });

                Progress = newValue;
                ScrollEnded?.Invoke();

                yield return StepTask.Delay(1);
                Hide();
            }
        }
        private class GradeElement : VisualElement
        {
            public string Letter { get; set; }

            private StepTask dropTask = null;

            public void SetGrade(string newLetter)
            {
                if (newLetter == null)
                {
                    IsGradeMode = false;
                    return;
                }

                IsGradeMode = true;
                Letter = newLetter;
                
                Alpha = 0;
               
                StepTask.Replace(ref dropTask, DropDown);
            }

            private IEnumerator DropDown()
            {
                Vector2 startScale = new(7);
                Vector2 endScale = new(1.7f);
                Scale = startScale;

                yield return StepTask.Interpolate((ref float e) =>
                {
                    Scale = Vector2.SmoothStep(startScale, endScale, e);
                    Alpha = (byte)MathHelper.Lerp(0, 255, e);

                    float time = 0.7f - e;

                    e += FrameState.DeltaTime / time.Clamp01();
                });

                Scale = endScale;

                yield return MoveScreen();
            }
            private static IEnumerator MoveScreen()
            {
                int gradeIndex = Storage.CompletionGrade.Index;

                Vector2 startPosition = reqVisual.Position;

                Camera.Position += -(Window.Center - Storage.Position).Normalized() * (gradeIndex * 2).ClampMin(3);
                yield return StepTask.Interpolate((ref float e) =>
                {
                    Camera.Position = Vector2.Lerp(Camera.Position, Vector2.Zero, e);

                    e += FrameState.DeltaTime;
                });
            }
        }

        private static SpriteFont BracketsFont => Fonts.Silk;
        private static SpriteFont Font => Fonts.SilkBold;

        private static Vector2 Position { get; set; } 
       
        private static CodeStorage Storage { get; set; } = null;

        private static RequirementElement reqVisual = new();
        private static ProgressElement progressDisplay = new();
        private static GradeElement grade = new();


        private static bool IsGradeMode { get; set; } = false;

        [Init]
        private static void Init()
        {
            Drawer.Register(Draw);
            Level.Created += () =>
            {
                Storage = Level.GetObject<CodeStorage>();

                Position = Storage.Position;

                Storage.RequirementChanged += reqVisual.SetLetter;
                reqVisual.SetLetter(Storage.Requirement);
                Storage.ProgressChanged += (d) => UpdateProgress();
                Storage.Filled += () =>
                {
                    Action showGrade = () =>
                    {
                        grade.SetGrade(Storage.GetGrade());
                        progressDisplay.Hide();
                    };
                    showGrade.Invoke(w => progressDisplay.ScrollEnded += w, w => progressDisplay.ScrollEnded -= w);
                };

                IsGradeMode = false;
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

            DrawTimer(context);
            DrawProgress(context);

            if (IsGradeMode)
            {
                context.String(
                    Font,
                    grade.Letter,
                    grade.Position + Position,
                    grade.ScaledColor,
                    Font.MeasureString(grade.Letter) / 2,
                    grade.Scale);

                return;
            }

            DrawStorage(context);
            
        }
        
        private static void DrawStorage(DrawContext context)
        {
            Vector2 offset = (new Vector2(15, 0) * reqVisual.Scale).WhereX(x => x.ClampMin(5));

            DrawOptions options = new()
            {
                origin = BracketsFont.MeasureString("[") / 2,
                font = BracketsFont,
                scale = new(1.4f, 2)
            };

            context.String(
                Font,
                reqVisual.Letter,
                Position + reqVisual.Position,
                reqVisual.ScaledColor,
                Font.MeasureString(reqVisual.Letter) / 2,
                reqVisual.Scale);

            options.position = Position - offset;
            context.String("[", options);

            offset.X -= 2;
            options.position = Position + offset;
            context.String("]", options);
        } 
        private static void DrawTimer(DrawContext context)
        {
            string timerText = Storage.Timer.ToString();

            context.String(
                Font,
                timerText,
                Position.WhereY(y => y - 40),
                Palette.White,
                Font.MeasureString(timerText) / 2,
                Vector2.One);
        }
        private static void DrawProgress(DrawContext context)
        {
            string progressStr = $"{(int)progressDisplay.Progress.Rounded()}/{Storage.Capacity}";

            context.String(
                Font,
                progressStr,
                Position.WhereY(y => y + 40) + progressDisplay.Position,
                progressDisplay.ScaledColor,
                Font.MeasureString(progressStr) / 2,
                new(1f));
        }

        private static void UpdateProgress()
        {
            //float progress = 100 * (Storage.Progress / Storage.Capacity);
            progressDisplay.SetProgress(Storage.Progress);
        }
    }
}