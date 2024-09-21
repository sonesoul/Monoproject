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

        private StepTask moveTask;
        private StepTask moveBackTask;

        private string text = "";
        private int textIndex = 0;

        private Vector2 textOrigin;
        private Vector2 drawOffset = Vector2.Zero;

        private Collider trigger;

        private IngameDrawer drawer;
        private SpriteBatch spriteBatch;

        private Vector2 smoothDistance = new(0, -50);
        private float smoothSpeed = 3;

        private static char EmptyChar => '-';
        private static SpriteFont Font => UI.SilkBold;
        
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
                return;

            IsInitialized = true;

            trigger = AddModule(new Collider()
            {
                Mode = ColliderMode.Trigger,
                polygon = Polygon.Rectangle(textOrigin.X * 2, 30)
            });

            drawer.AddDrawAction(Draw);

            trigger.TriggerEnter += c =>
            {
                if (c.Owner is not Player player)
                    return;

                player.CanCombinate = true;

                moveBackTask?.Break();
                moveTask = new(Move(smoothDistance), true);
            };
            trigger.TriggerExit += c =>
            {
                if (c.Owner is not Player player)
                    return;

                player.CanCombinate = false;
                
                moveTask?.Break();
                moveBackTask = new(MoveBack(), true);
            };
        }
        public void Destruct() => Destroy();

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
                elapsed += FrameInfo.DeltaTime * smoothSpeed;

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
                elapsed += FrameInfo.DeltaTime * smoothSpeed;

                yield return null;
            }
            drawOffset = Vector2.Zero;
        }

        private void Draw(GameTime gt)
        {
            spriteBatch.DrawString(
                Font,
                text,
                Position + drawOffset,
                TextColor,
                0,
                textOrigin,
                new Vector2(1f, 1f),
                SpriteEffects.None,
                0
            );
        }

        private void UpdateOrigin() => textOrigin = Font.MeasureString(text) / 2;

        protected override void PostDestroy()
        {
            drawer.RemoveDrawAction(Draw);

            trigger.TriggerEnter -= _ =>
            {
                moveBackTask?.Break();
                moveTask = new(Move(smoothDistance), true);
            };
            trigger.TriggerExit -= _ =>
            {
                moveTask?.Break();
                moveBackTask = new(MoveBack(), true);
            };
        }
    }
}