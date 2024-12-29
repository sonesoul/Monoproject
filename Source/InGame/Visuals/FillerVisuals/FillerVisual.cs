using Engine.Drawing;
using GlobalTypes;
using InGame.GameObjects;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Linq;

namespace InGame.Visuals.FillerVisuals
{
    public static class FillerVisual
    {
        private class CodeUIElement
        {
            public string FirstChar { get; set; }
            public string RestCode { get; set; }

            public static string NullChar => "-";

            public Vector2 CharPosition { get; set; }
            public Vector2 CodeOffset { get; set; }
            
            public Vector2 CharOrigin => Font.MeasureString(FirstChar) / 2;
            public Vector2 CodeOrigin => (Font.MeasureString(RestCode) / 2).WhereX(0);

            public Vector2 CharScale { get; set; } = Vector2.One;
            public Vector2 CodeScale { get; set; } = new Vector2(0.7f);

            public Color charColor = Palette.White;
            public Color codeColor = new(Palette.White, 0);

            public SpriteFont Font { get; set; }

            private int index = 0;
            private string targetCode;

            private byte visibleCodeAlpha = 155;

            public CodeUIElement()
            {
                Font = Fonts.SilkBold;

                SetTarget(null);
            }

            private StepTask charDropTask = null;
            private StepTask codeMoveTask = null;
            private StepTask codeAlphaLerp = null;

            public void SetTarget(Code? code)
            {
                index = 0;
                if (code.TryGetValue(out var c)) 
                {
                    targetCode = c.ToString();
                }
                else
                {
                    targetCode = NullChar.Times(LevelConfig.CodeLength);
                }
                UpdateCode();
            }
            public void MoveNext()
            {
                CodeOffset += Font.MeasureString(FirstChar).WhereY(0) * CharScale;
                index++;

                if (index >= targetCode.Length)
                    SetTarget(null);

                UpdateCode();

                if (FirstChar != NullChar)
                {
                    StepTask.Replace(ref charDropTask, DropDown);
                }

                StepTask.Replace(ref codeMoveTask, MoveRestCode);
            }

            public void ShowCode()
            {
                codeAlphaLerp?.Break();
                codeColor.A = visibleCodeAlpha;
            }
            public void HideCode()
            {
                codeAlphaLerp?.Break();
                codeColor.A = 0;
            }

            public void Shake()
            {
                string[] chars = new[] { "?", "!", "?!", ":(", "F-", "F1", "P", "X", };
                FirstChar = chars.RandomElement();
                RestCode = " ";

                CodeOffset = Vector2.Zero;
                StepTask.Run(MistakeShake());
            }

            private void UpdateCode()
            {
                FirstChar = targetCode[index].ToString();
                RestCode = targetCode[(index + 1)..].ToString();
            }
            private IEnumerator MistakeShake()
            {
                Vector2 radius = new(3, 3);
                Random random = new();

                yield return StepTask.Interpolate((ref float e) =>
                {
                    CharPosition = new(random.Next((int)-radius.X, (int)radius.X), random.Next((int)-radius.Y, (int)radius.Y));

                    e += FrameState.DeltaTime / 0.1f;
                });
                
                CharPosition = Vector2.Zero;
            }
            private IEnumerator MoveRestCode()
            {
                while (CodeOffset.Length() > 1)
                {
                    CodeOffset = Vector2.Lerp(CodeOffset, Vector2.Zero, 0.2f);

                    yield return null;
                }

                CodeOffset = Vector2.Zero;
            }
            private IEnumerator DropDown()
            {
                Vector2 dropDistance = new(0, 6);
                CharPosition = dropDistance;

                while (CharPosition.Length() > 1)
                {
                    CharPosition = Vector2.Lerp(CharPosition, Vector2.Zero, 0.1f);
                    yield return null;
                }

                CharPosition = Vector2.Zero;
            }
        }

        private static CodeUIElement visual = new();
        private static StorageFiller filler;
        private static bool isShaking = false;

        [Init]
        private static void Init()
        {
            Drawer.Register(Draw);

            Level.Created += () =>
            {
                visual.SetTarget(null);
                visual.codeColor.A = 0;

                filler = Level.GetObject<StorageFiller>();

                if (filler == null)
                    return;

                filler.TargetCodeChanged += (c) => UpdateState();
                filler.CharAdded += (c) => visual.MoveNext();

                filler.MistakeOccured += () =>
                {
                    visual.Shake();
                    isShaking = true;
                };
                filler.InputEnabled += () =>
                {
                    isShaking = false;
                    UpdateState();
                };

                //filler.Activated += () => visual.codeColor.A = 150;
                //filler.Deactivated += () => visual.codeColor.A = 0;
                filler.Activated += visual.ShowCode;
                filler.Deactivated += visual.HideCode;
            };

            Level.Cleared += () =>
            {
                filler = null;
            };
        }

        private static void Draw(DrawContext context)
        {
            if (filler == null)
                return;

            Vector2 targetPos = filler.Position.Where((x, y) => new Vector2(x - 1, y - 30));

            string stand = @"/\";
            var font = visual.Font;

            context.Line(filler.Position, filler.Storage.Position, new(Palette.White, (byte)(visual.codeColor.A / 2)), 2);

            context.String(Fonts.SilkBold, stand, filler.Position, Palette.White, Fonts.SilkBold.MeasureString(stand) / 2, new(1));

            context.String(
                font,
                visual.FirstChar,
                targetPos + visual.CharPosition + visual.CodeOffset,
                visual.charColor,
                visual.CharOrigin,
                visual.CharScale);

            context.String(
                font,
                visual.RestCode,
                targetPos + visual.CodeOffset + new Vector2(10, 0),
                visual.codeColor,
                visual.CodeOrigin,
                visual.CodeScale);
        }

        private static void UpdateState()
        {
            if (filler == null || isShaking)
                return;

            visual.SetTarget(filler.TargetCode);
        }
    }
}