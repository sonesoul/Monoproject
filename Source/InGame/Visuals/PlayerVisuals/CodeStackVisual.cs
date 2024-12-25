using Engine.Drawing;
using GlobalTypes.Events;
using GlobalTypes;
using InGame.GameObjects;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;

namespace InGame.Visuals.PlayerVisuals
{
    public static class CodeStackVisual
    {
        private class CodeUIElement
        {
            public Code Code { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 TargetPosition { get; set; }

            public Vector2 Scale { get; set; }

            public float Alpha { get; set; } = 1f;

            public bool IsDisappearing { get; private set; }

            public event Action<CodeUIElement> Disappeared;

            private StepTask disappearTask = null;
            private StepTask moveTask = null;

            public CodeUIElement(Code code, Vector2 position)
            {
                Code = code;
                Position = position;
                TargetPosition = position;
            }

            public void Move(Vector2 newPosition)
            {
                StepTask.Replace(ref moveTask, LerpPosition(newPosition));
            }

            public void Disappear()
            {
                if (IsDisappearing)
                    return;

                disappearTask = StepTask.Run(ChangeAlpha(0, 0.1f));

                IsDisappearing = true;
            }

            private IEnumerator ChangeAlpha(float newAlpha, float seconds)
            {
                float elapsed = 0;

                float start = Alpha;

                while (elapsed < 1)
                {
                    elapsed += FrameState.DeltaTime / seconds;

                    Alpha = MathHelper.Lerp(start, newAlpha, elapsed);

                    yield return null;
                }

                Disappeared?.Invoke(this);
            }

            private IEnumerator LerpPosition(Vector2 newPosition)
            {
                while (Position.DistanceTo(newPosition) > 1)
                {
                    Position = Vector2.Lerp(Position, newPosition, 0.2f);
                    yield return null;
                }

                Position = newPosition;
            }

            public void Dispose() 
            {
                Disappeared = null;
            }
        }

        private static readonly List<CodeUIElement> displays = new();
        private static Vector2 startPosition = new(10, 10);
        
        private static Player player = null;

        [Init]
        private static void Init()
        {
            Drawer.Register(DrawCodes, true);

            Player.Created += (p) =>
            {
                player = p;
                displays.Clear();

                p.Destroyed += i =>
                {
                    if (player == i)
                    {
                        player = null;
                    }
                };

                p.Codes.Pushed += AddCode;
                p.Codes.Popped += RemoveCode;
            };
        }

        private static void DrawCodes(DrawContext context)
        {
            if (player == null)
                return;

            int index = 1;
            int alphaIgnore = displays.Count - player.Codes.StackSize;

            foreach (var display in displays)
            {
                Color color = (Palette.White * display.Alpha);

                if (alphaIgnore-- <= 0)
                    color.A /= (byte)index++;

                context.String(
                    Fonts.SilkBold, 
                    display.Code.ToString(),
                    display.Position, 
                    color, 
                    Vector2.Zero, 
                    display.Scale);
            }
        }

        public static void AddCode(Code code)
        {
            Vector2 position = startPosition + new Vector2(0, 0);
            displays.Insert(0, new CodeUIElement(code, position));
            UpdateTargetPositions();
        }
        public static void RemoveCode(Code code)
        {
            var display = displays.FirstOrDefault(c => c.Code == code && !c.IsDisappearing);
            if (display != null)
            {
                display.Disappear();
                display.Disappeared += (d) =>
                {
                    displays.Remove(d);
                    d.Dispose();
                    UpdateTargetPositions();
                };
            }
        }

        private static void UpdateTargetPositions()
        {
            if (player == null)
                return;

            Vector2 lastPos = startPosition;
            Vector2 lastSize = Vector2.Zero;

            int scaleIgnore = displays.Count - player.Codes.StackSize;

            for (int i = 0; i < displays.Count; i++)
            {
                int j = i;

                if (j < scaleIgnore)
                {
                    j = 0;
                }
                else if (scaleIgnore > 0)
                {
                    j -= scaleIgnore; 
                }

                Vector2 scale = displays[i].Scale = new Vector2((1.0f - 0.2f * j).Clamp(0.6f, 1));
                Code code = displays[i].Code;

                lastPos += lastSize.WhereX(0);
                lastSize = Fonts.SilkBold.MeasureString(code.Sequence) * scale;

                displays[i].Move(lastPos);
            }
        }
    }
}