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
    public class StorageFiller : ModularObject, ITaggable
    {
        public string Tag => "filler";
        public bool IsFilled => inputIndex >= maxLength;
        private readonly WordStorage storage;

        private string text;
        private int inputIndex = 0;
        private readonly char emptyChar = '-';
        private Vector2 inputOrigin;
        private Vector2 drawOffset = Vector2.Zero;
        
        private Color inputColor = Color.White;
        private Collider collider;
        private int maxLength;

        private StepTask moveTask;
        private StepTask moveBackTask;

        private Vector2 end => new(0, -50);
        private float smoothSpeed = 3;
        private static IngameDrawer Drawer => IngameDrawer.Instance;

        public StorageFiller(WordStorage storage, int maxLength)
        {
            this.maxLength = maxLength;
            this.storage = storage;
            Drawer.AddDrawAction(Draw);
            text = emptyChar.Times(maxLength);
            UpdateOrigin();
            
            collider = AddModule(new Collider(this)
            {
                Mode = ColliderMode.Trigger,
                polygon = Polygon.Rectangle(inputOrigin.X * 2, 30)
            });

            collider.TriggerEnter += c =>
            {
                moveBackTask?.Break();
                moveTask = new(Move(end), true);
            };
            collider.TriggerExit += c =>
            {
                moveTask?.Break();
                moveBackTask = new(MoveBack(), true);
            };
        }

        private void Draw(GameTime gt)
        {
            Drawer.SpriteBatch.DrawString(
                UI.Font, 
                text, 
                position + drawOffset, 
                inputColor,
                0,
                inputOrigin,
                new Vector2(0.8f, 0.8f),
                SpriteEffects.None, 
                0
                );
        }
 
        public void Append(char c)
        {
            if (!char.IsLetter(c))
                return;
            
            c = char.ToUpper(c);

            if (WordStorage.AlphabetUpper.Contains(c) && !IsFilled)
            {
                text = text.CharAt(inputIndex++, c);
                UpdateOrigin();
            }
        }
        public void Backspace()
        {
            if (inputIndex > 0)
            {
                text = text.CharAt(--inputIndex, emptyChar);
                UpdateOrigin();
            }
        }
        public bool Push()
        {
            if (IsFilled)
            {
                if (storage.Push(text))
                {
                    text = emptyChar.Times(maxLength);
                    inputIndex = 0;
                    UpdateOrigin();        
                    return true;
                }
            }

            return false;
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

        private void UpdateOrigin() => inputOrigin = UI.Font.MeasureString(text) / 2;
        protected override void PostDestroy()
        {
            Drawer.RemoveDrawAction(Draw);
        }
    }
}