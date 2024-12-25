using Engine.Drawing;
using GlobalTypes;
using InGame.GameObjects;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace InGame.Visuals.PlayerVisuals
{
    public static class ResourceVisual
    {
        private class ShowBar
        {
            public string Text { get; set; }

            public Color DrawColor { get; set; }
            public Vector2 Position { get; set; }
            public float Scale { get; set; }

            public Color BaseColor { get; set; } = Palette.White;
            public Vector2 BasePosition { get; set; } = Vector2.Zero;
            public float BaseScale { get; set; } = 0.7f;


            private StepTask alphaChange = null;
            private StepTask scaleImpulse = null;
            private StepTask dropDown = null;

            public ShowBar()
            {
                Position = BasePosition;
                Scale = BaseScale;
                DrawColor = BaseColor;
            }

            public void Show(float seconds)
            {
                StepTask.Replace(ref alphaChange, LerpColor(BaseColor, seconds));
            }
            public void Hide(float seconds)
            {
                StepTask.Replace(ref alphaChange, LerpColor(new Color(BaseColor, 0), seconds));
            }

            public void DropDown(float distance, float dropTime, float upTime)
            {
                StepTask.Replace(ref dropDown, ImpulseDown(distance, dropTime, upTime));
            }

            public void ImpulseScale(float scale, float seconds)
            {
                StepTask.Replace(ref scaleImpulse, ScaleImpulse(scale, seconds));
            }

            private IEnumerator ScaleImpulse(float newScale, float seconds)
            {
                float elapsed = 0;
                Scale = newScale;

                while (elapsed < 1)
                {
                    elapsed += FrameState.DeltaTime / seconds;
                    Scale = MathHelper.Lerp(newScale, BaseScale, elapsed);

                    yield return null;
                }
            }

            private IEnumerator ImpulseDown(float distance, float dropTime, float upTime)
            {
                float elapsed = 0;

                Vector2 start = Position;
                Vector2 end = new(0, distance);

                while (elapsed < 1)
                {
                    elapsed += FrameState.DeltaTime / dropTime;

                    Position = Vector2.Lerp(start, end, elapsed);
                    yield return null;
                }

                elapsed = 0;

                while (elapsed < 1)
                {
                    elapsed += FrameState.DeltaTime / upTime;

                    Position = Vector2.Lerp(end, BasePosition, elapsed);
                    yield return null;
                }
            }
            private IEnumerator LerpColor(Color end, float seconds)
            {
                float elapsed = 0f;
                Color start = DrawColor;

                while (elapsed < 1f) 
                {
                    elapsed += FrameState.DeltaTime / seconds;

                    DrawColor = Color.Lerp(start, end, elapsed);
                    
                    yield return null;
                }

                DrawColor = end;
                alphaChange = null;
            } 
        }

        public static float BaseSmoothSpeed => 5f;
        public static Vector2 Position { get; private set; }
        public static bool IsManuallyShown { get; private set; } = false;

        public static float SmoothSpeed { get; set; } = BaseSmoothSpeed;

        private static SpriteFont font = Fonts.Silk;

        private static string icon = ".";
        private static Vector2 iconOrigin;

        private static Player player;

        private readonly static ShowBar bitBar = new();

        private readonly static StepTask bitsArrive = new(() => ShowForSeconds(2, bitBar));
        private readonly static StepTask positionUpdate = StepTask.Run(UpdatePosition());

        [Init]
        private static void Init()
        {
            Drawer.Register(Draw);

            Player.ShowUIRequested += Show;
            Player.HideUIRequested += Hide;

            Player.Created += p =>
            {
                player = p;

                player.Destroyed += p =>
                {
                    if (player == p)
                    {
                        player = null;
                    }
                };

                var wallet = p.BitWallet;
                wallet.ValueChanged += SetBits;

                bitBar.Text = $"{wallet.BitCount}";
                bitBar.Position = player.Position;
                
                iconOrigin = font.MeasureString(icon) / 2;

                bitBar.Hide(0);
            };
        }

        private static void Draw(DrawContext context)
        {
            if (player == null)
                return;

            Vector2 bitCenter = (font.MeasureString(bitBar.Text) / 2).WhereY(0);
            Vector2 position = Position.WhereY(y => y - 40) + bitBar.Position;

            context.String(
                font, 
                bitBar.Text,
                position,
                bitBar.DrawColor,
                bitCenter,
                new Vector2(bitBar.Scale));

            context.String(
                font,
                icon,
                position.WhereX(x => x - (bitCenter.X * bitBar.Scale) - 7),
                bitBar.DrawColor,
                iconOrigin,
                new Vector2(bitBar.Scale * 3f));
        }

        private static void SetBits(int newBits)
        {
            if (bitBar.Text.HasContent())
            {
                int countBefore = bitBar.Text.ToInt();

                if (countBefore > newBits)
                {
                    bitBar.DropDown(8, 0.03f, 0.15f);
                }
                else
                {
                    bitBar.ImpulseScale(1, 0.3f);
                }
            }

            bitBar.Text = $"{newBits}";
            bitsArrive.Restart();
        }


        private static void Show()
        {
            float seconds = 0.1f;
            bitBar.Show(seconds);

            SmoothSpeed *= 3;
            IsManuallyShown = true;
        }
        private static void Hide()
        {
            float seconds = 0.1f;
            bitBar.Hide(seconds);

            SmoothSpeed = BaseSmoothSpeed;
            IsManuallyShown = false;
        }

        private static IEnumerator ShowForSeconds(float seconds, ShowBar bar)
        {
            if (IsManuallyShown)
                yield break;

            bar.Show(0.1f);
            yield return StepTask.Delay(seconds);

            if (IsManuallyShown)
                yield break;
            bar.Hide(0.1f);
        }
        private static IEnumerator UpdatePosition()
        {
            while (true)
            {
                if (player == null)
                {
                    yield return null;
                    continue;
                }

                Vector2 playerPos = player.IntegerPosition;
                Position = Vector2.Lerp(Position, playerPos, SmoothSpeed * FrameState.DeltaTime);

                yield return null;
            }
        }
    }
}