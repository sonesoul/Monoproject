using Engine;
using Engine.Drawing;
using GlobalTypes;
using GlobalTypes.InputManagement;
using GlobalTypes.Interfaces;
using InGame.Visuals;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InGame.Overlays
{
    public class BindButton : VisualElement, IDestroyable
    {
        private class KeyElement : VisualElement
        {
            public Vector2 Size => BracketsSize * Scale;

            public string Brackets { get; private set; }
            public Vector2 BracketsSize { get; private set; }
            public Vector2 CharSize { get; private set; }
            public string KeyChar { get; private set; }

            private SpriteFont font;
            private int bracketSpacing = 2;

            public KeyElement(string keyChar, SpriteFont font)
            {
                this.font = font;
                SetChar(keyChar);
            }

            public void SetChar(string newChar)
            {
                KeyChar = newChar;
                Brackets = $@"({" ".Times(bracketSpacing * 3)})";

                BracketsSize = font.MeasureString(Brackets);
                CharSize = font.MeasureString(KeyChar);
            }
        }

        public bool IsDestroyed { get; set; }
        public bool Enabled { get; set; } = true;

        public Vector2 Size => (keyElement.Size + otherTextSize.TakeX()) * Scale;

        public event Action Triggered;

        private string text, firstChar, otherText;
        private SpriteFont font = Fonts.PicoMono;

        private KeyElement keyElement;

        private Vector2 finalPosition;
        private Vector2 otherTextSize;

        private Key triggerKey;

        public BindButton(string text, int layer = -1)
        {
            if (text.Length <= 2)
                throw new ArgumentException("Button text must be longer than two characters.");

            if (!char.IsLetter(text[0]))
                throw new ArgumentException("First char of the button text must be a letter.", nameof(text));

            this.text = text;
            firstChar = text[0].ToString().ToUpper();
            otherText = text[1..].ToLower();

            keyElement = new(firstChar, font);

            otherTextSize = font.MeasureString(otherText);

            triggerKey = Enum.Parse<Key>(firstChar);

            Input.KeyPressed += OnKeyPressed;
            Input.KeyReleased += OnKeyReleased;
            Drawer.Register(Draw, false, layer);
        }

        private void OnKeyPressed(Key key)
        {
            if (key != triggerKey || !Enabled)
                return;

            keyElement.Scale = new(0.8f);
            keyElement.Position = new(0, 3);

            Sfx.Play(Sounds.ButtonPress);
        }
        private void OnKeyReleased(Key key)
        {
            if (key != triggerKey || !Enabled)
                return;
            Triggered?.Invoke();
        }

        public void Draw(DrawContext context)
        {
            //starting from left bracket
            finalPosition = Position + (keyElement.Size / 2 * Scale);

            //centralizing
            finalPosition -= Size / 2;

            DrawKey(context, finalPosition);

            Vector2 spacing = new(font.Spacing, 0);
            Vector2 textOffset = keyElement.Size / 2 + spacing;

            context.String(
                font,
                otherText,
                finalPosition + textOffset.TakeX() * Scale,
                ScaledColor,
                otherTextSize.TakeY() / 2,
                Scale);
        }

        private void DrawKey(DrawContext context, Vector2 position)
        {
            //brackets
            context.String(
                font,
                keyElement.Brackets,
                position + keyElement.Position,
                new(ScaledColor, (byte)(Alpha / 2)),
                keyElement.BracketsSize / 2,
                keyElement.Scale * Scale);

            //key
            context.String(
                font,
                keyElement.KeyChar,
                position + keyElement.Position,
                ScaledColor,
                keyElement.CharSize / 2,
                keyElement.Scale * Scale);
        }

        public void Destroy() => IDestroyable.Destroy(this);        
        public void ForceDestroy() => Drawer.Unregister(Draw);
    }
}