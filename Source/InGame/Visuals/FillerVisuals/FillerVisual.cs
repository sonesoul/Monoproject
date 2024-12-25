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

            public float CodeAlpha { get; set; } = 0;

            public SpriteFont Font { get; set; }

            private int index = 0;
            private string targetCode;

            public CodeUIElement(SpriteFont font)
            {
                Font = font;
                SetTarget(null);
            }

            private StepTask charDropTask = null;
            private StepTask codeMoveTask = null;
            
            public void SetTarget(Code? code)
            {
                index = 0;
                if (code.TryGetValue(out var c)) 
                {
                    targetCode = c.ToString();
                }
                else
                {
                    targetCode = NullChar.Times(LevelConfig.CodeSize);
                }

                UpdateCode();
            }
            public void MoveNext()
            {
                CodeOffset = Font.MeasureString(FirstChar).WhereY(0);
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
            
            public void Shake()
            {
                string[] chars = new[] { "?", "!", "?!", ":<", "F-", "F1", "^^", "X", };

                FirstChar = chars.RandomElement();
                RestCode = " ";

                CodeOffset = Vector2.Zero;
                StepTask.Run(FailureShake());
            }

            private void UpdateCode()
            {
                FirstChar = targetCode[index].ToString();
                RestCode = targetCode[(index + 1)..].ToString();
            }

            private IEnumerator FailureShake()
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
                Vector2 dropDistance = new(0, 7);

                CharPosition = dropDistance / 2;
                yield return null;
                CharPosition = dropDistance;
                yield return null;

                while (CharPosition.Length () > 1)
                {
                    CharPosition = Vector2.Lerp(CharPosition, Vector2.Zero, 0.1f);
                    yield return null;
                }

                CharPosition = Vector2.Zero;
            }
        }

        private static CodeUIElement visual = new(Fonts.SilkBold);
        private static StorageFiller filler;
        private static bool isShaking = false;

        [Init]
        private static void Init()
        {
            Drawer.Register(Draw);

            Level.Created += () =>
            {
                visual.SetTarget(null);
                visual.CodeAlpha = 0;

                filler = Level.GetObject<StorageFiller>();

                if (filler == null)
                    return;

                filler.TargetCodeChanged += (c) => UpdateState();
                filler.CharAdded += (c) => visual.MoveNext();

                filler.InputFailed += () =>
                {
                    visual.Shake();
                    isShaking = true;
                };
                filler.InputEnabled += () =>
                {
                    isShaking = false;
                    UpdateState();
                };

                filler.Activated += () => visual.CodeAlpha = 150;
                filler.Deactivated += () => visual.CodeAlpha = 0;
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

            context.Line(filler.Position, filler.Storage.Position, new(Palette.White, (byte)(visual.CodeAlpha / 2)), 2);

            context.String(font, stand, filler.Position, Palette.White, font.MeasureString(stand) / 2, new(1));

            context.String(
                font,
                visual.FirstChar,
                targetPos + visual.CharPosition + visual.CodeOffset,
                Palette.White,
                visual.CharOrigin,
                new Vector2(1));

            context.String(
                font,
                visual.RestCode,
                targetPos + visual.CodeOffset + new Vector2(10, 0),
                new Color(Palette.White, (byte)visual.CodeAlpha),
                visual.CodeOrigin,
                new Vector2(0.7f));
        }

        private static void UpdateState()
        {
            if (filler == null || isShaking)
                return;

            visual.SetTarget(filler.TargetCode);
        }
    }
}