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
    public class StorageFiller : ModularObject, ILevelObject, IFillable
    {
        public string Tag => nameof(StorageFiller);
        public bool IsInitialized { get; private set; } = false;

        public ComboStorage Storage { get; set; }
        public Color TextColor { get; set; } = Color.White;

        public string CurrentCombo => text[..textIndex];
        public int Size { get; private set; }

        public bool IsFilled => textIndex >= Size;

        private StepTask moveTask;
        private StepTask moveBackTask;

        private string text = "";
        private int textIndex = 0;

        private Vector2 textOrigin;
        private Vector2 drawOffset = Vector2.Zero;

        private Collider collider;

        private Vector2 smoothDistance = new(0, -50);
        private float smoothSpeed = 3;

        private static char EmptyChar => '-';
        private static SpriteFont Font => UI.SilkBold;
        
        public StorageFiller(ComboStorage storage, int size)
        {
            Storage = storage;
            Size = size;

            Clear();
        }

        public void Init()
        {
            if (IsInitialized)
                return;

            IsInitialized = true;

            collider = AddModule(new Collider()
            {
                Shape = Polygon.Rectangle(textOrigin.X * 2, 30)
            });

            Drawer.Register(Draw, true);

            moveTask = new(Move);
            moveBackTask = new(MoveBack);

            collider.OnOverlapEnter += OnTriggerEnter;
            collider.OnOverlapExit += OnTriggerExit;
        }
        public void Destruct() => Destroy();

        public bool Push()
        {
            if (IsFilled)
            {
                bool isPushed = Storage.Push(new Combo(text));

                Clear();

                return isPushed;
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
        public void Clear()
        {
            textIndex = 0;
            text = EmptyChar.Times(Size);
            UpdateOrigin();
        }

        private IEnumerator Move()
        {
            Vector2 end = smoothDistance;

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

        private void OnTriggerEnter(Collider c)
        {
            if (c.Owner is not Player player)
                return;

            player.CanCombinate = true;
            player.CanRollCombos = false;

            moveBackTask?.Break();
            moveTask.Start();
        }
        private void OnTriggerExit(Collider c)
        {
            if (c.Owner is not Player player)
                return;

            player.CanCombinate = false;
            player.CanRollCombos = true;

            moveTask?.Break();
            moveBackTask.Start();
        }

        private void Draw(DrawContext context)
        {
            context.String(
                Font,
                text,
                Position + drawOffset,
                TextColor,
                0,
                textOrigin,
                new Vector2(1f, 1f)
            );
        }

        private void UpdateOrigin() => textOrigin = Font.MeasureString(text) / 2;

        protected override void PostDestroy()
        {
            Drawer.Unregister(Draw);

            collider.OnOverlapEnter -= OnTriggerEnter;
            collider.OnOverlapExit -= OnTriggerExit;
        }
    }
}