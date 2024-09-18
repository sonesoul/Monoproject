using Engine;
using Engine.Drawing;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using InGame.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Linq;

namespace InGame.GameObjects
{
    public class StorageFiller : ModularObject, ILevelObject
    {
        public string Tag => nameof(StorageFiller);
        public bool IsInitialized { get; private set; } = false;

        public ComboStorage Storage { get; set; }
        public Color TextColor { get; set; } = Color.White;

        public string CurrentCombo => text[..textIndex];
        public int MaxLength { get; private set; }

        public bool IsFilled => textIndex >= MaxLength;

        private char EmptyChar => '-';

        private Vector2 SmoothPower { get; set; } = new(0, -50);
        private float SmoothSpeed { get; set; } = 3;

        private StepTask moveTask;
        private StepTask moveBackTask;
       
        private string text = "";
        private int textIndex = 0;

        private Vector2 textOrigin;
        private Vector2 drawOffset = Vector2.Zero;

        private Collider trigger;

        private SpriteFont TextFont => UI.SilkBold;
        
        private IngameDrawer drawer;
        private SpriteBatch spriteBatch;
        
        public StorageFiller(ComboStorage storage, int maxLength)
        {
            Storage = storage;
            MaxLength = maxLength;
            
            text = EmptyChar.Times(maxLength);
            UpdateOrigin();

            drawer = IngameDrawer.Instance;
            spriteBatch = drawer.SpriteBatch;
        }

        public void Init()
        {
            if (IsInitialized)
                throw new InvalidOperationException("Can't be initialized twice.");

            IsInitialized = true;

            trigger = AddModule(new Collider()
            {
                Mode = ColliderMode.Trigger,
                polygon = Polygon.Rectangle(textOrigin.X * 2, 30)
            });

            drawer.AddDrawAction(Draw);

            trigger.TriggerEnter += c =>
            {

                /*if (c.Owner != Level.GetObject<Player>())
                    return;*/


                moveBackTask?.Break();
                moveTask = new(Move(SmoothPower), true);
            };
            trigger.TriggerExit += c =>
            {
                /*if (c.Owner != Level.GetObject<Player>())
                    return;*/

                moveTask?.Break();
                moveBackTask = new(MoveBack(), true);
            };
        }
        public void Terminate() => Destroy();

        public bool Push()
        {
            if (IsFilled)
            {
                return Storage.Push(new Combo(text));
            }

            return false;
        }
        public void Append(char c)
        {
            if (!char.IsLetter(c))
                return;

            c = char.ToUpper(c);

            if (Level.KeyPattern.Contains(c) && !IsFilled)
            {
                text = text.CharAt(textIndex++, c);
                UpdateOrigin();
            }
        }
        public void Backspace()
        {
            if (textIndex > 0)
            {
                text = text.CharAt(--textIndex, EmptyChar);
                UpdateOrigin();
            }
        }

        private IEnumerator Move(Vector2 end)
        {
            float elapsed = 0;
            while (elapsed < 1f)
            {
                drawOffset = Vector2.Lerp(drawOffset, end, elapsed);
                elapsed += FrameInfo.DeltaTime * SmoothSpeed;

                yield return null;
            }
            drawOffset = end;
        }
        private IEnumerator MoveBack()
        {
            float elapsed = 0;
            while (elapsed < 1f)
            {
                drawOffset = Vector2.Lerp(drawOffset, Vector2.Zero, elapsed);
                elapsed += FrameInfo.DeltaTime * SmoothSpeed;

                yield return null;
            }
            drawOffset = Vector2.Zero;
        }


        private void UpdateOrigin() => textOrigin = TextFont.MeasureString(text) / 2;

        private void Draw(GameTime gt)
        {
            spriteBatch.DrawString(
                TextFont,
                text,
                position + drawOffset,
                TextColor,
                0,
                textOrigin,
                new Vector2(1f, 1f),
                SpriteEffects.None,
                0
            );
        }

        protected override void PostDestroy()
        {
            drawer.RemoveDrawAction(Draw);

            trigger.TriggerEnter -= _ =>
            {
                moveBackTask?.Break();
                moveTask = new(Move(SmoothPower), true);
            };
            trigger.TriggerExit -= _ =>
            {
                moveTask?.Break();
                moveBackTask = new(MoveBack(), true);
            };
        }
    }
}