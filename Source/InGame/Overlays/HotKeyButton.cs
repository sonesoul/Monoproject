using Engine;
using Engine.Drawing;
using GlobalTypes;
using GlobalTypes.InputManagement;
using GlobalTypes.Interfaces;
using InGame.Visuals;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;

namespace InGame.Overlays
{
    public class HotKeyButton : IDestroyable
    {
        private class HotKeyVisual : VisualElement
        {
            public Vector2 BasePosition { get; set; } = Vector2.Zero;
            public Vector2 BaseScale { get; set; } = Vector2.One;

            public Vector2 Origin { get; set; }

            public bool IsTimeScaled { get; set; } = true;
            public string KeyChar { get; private set; }
            public SpriteFont Font { get; set; }

            private Vector2 delayedPosition;
            private Vector2 delayedScale;

            public HotKeyVisual(Key k, SpriteFont font)
            {
                Font = font;
                SetChar(k);

                Position = BasePosition;
                Scale = BaseScale;
            }

            public void OnClick(float pixels)
            {
                delayedPosition = new(0, pixels);
                delayedScale = new(0.8f, 0.8f);

                Position = delayedPosition;
                Scale = delayedScale;
            }
            public void OnRelease()
            {
                Position = delayedPosition;
                Scale = delayedScale;

                Position = BasePosition;
                Scale = BaseScale;
            }

            public void SetChar(Key k)
            {
                KeyChar = k.ToString().ToLower();
                UpdateOrigin();
            }

            private void UpdateOrigin()
            {
                Origin = Font.MeasureString(KeyChar) / 2;
            }
        }

        public event Action HotKeyPressed, HotKeyReleased;
        
        public bool IsDestroyed { get; set; } = false;

        public Vector2 Position { get; set; }
        public Color Color { get; set; } = Palette.White;
        public Vector2 Scale { get; set; } = Vector2.One;

        public Vector2 Size { get; set; }
        public bool Enabled { get; set; } = true;

        public bool IsTimeScaled
        {
            get => keyVisual.IsTimeScaled; 
            set => keyVisual.IsTimeScaled = value;
        }


        private string parsedText;
        private float textWidth;

        private Key hotKey;
        private HotKeyVisual keyVisual;
        private DrawOptions bracketOptions, textOptions;

        private Vector2 finalPosition;
        private Vector2 bracketOffset;

        private bool wasPressed = false;

        private SpriteFont TextFont { get; set; } = Fonts.SilkBold;
        private SpriteFont BracketFont { get; set; } = Fonts.Silk;

        public HotKeyButton(string text, int layer = -1)
        {
            Input.KeyPressed += OnKeyPress;
            Input.KeyReleased += OnKeyRelease;
            SetText(text);

            Drawer.Register(Draw, false, layer);

            Scale = new(1);
        }
        
        private void Draw(DrawContext context)
        {
            UpdateOptions();

            DrawBrackets(context, finalPosition, bracketOffset);
            context.String(parsedText, textOptions);

            context.String(
                keyVisual.Font,
                hotKey.ToString(),
                finalPosition + keyVisual.Position + (bracketOffset / 2),
                Color,
                keyVisual.Origin,
                keyVisual.Scale * Scale);
        }
        private void DrawBrackets(DrawContext context, Vector2 position, Vector2 offset)
        {
            bracketOptions.position = position + keyVisual.Position;
            context.String("(", bracketOptions);

            bracketOptions.position += offset.WhereX(x => x + 0.5f);
            context.String(")", bracketOptions);
        }
        private void UpdateOptions()
        {
            Vector2 bracketScale = new Vector2(1.4f, 2) * Scale * keyVisual.Scale;
            Vector2 bracketDistance = new(11, 0);
            Vector2 bracketOrigin = new(3.5f, 14);

            textWidth = (TextFont.MeasureString(parsedText) * Scale).X;

            bracketOffset = bracketDistance * bracketScale * 2;
            finalPosition = Position - bracketOffset.WhereX(x => x + textWidth) / 2;

            bracketOptions = new()
            {
                origin = bracketOrigin,
                font = BracketFont,
                scale = bracketScale,
                color = Color
            };

            textOptions = new()
            {
                origin = bracketOrigin.WhereX(0),
                font = TextFont,
                scale = Scale,
                color = Color
            };

            Vector2 bracketsSize = finalPosition + bracketOffset;
           
            textOptions.position = bracketsSize + new Vector2(bracketOrigin.X * 2, bracketOrigin.Y * 0.3f);

            Vector2 origin = new Vector2(5, 18) * Scale; 
            Vector2 totalSize = (finalPosition + bracketOffset).WhereX(x => x + textWidth) + new Vector2(2, -6) * Scale;

            Size = (totalSize + origin) - (finalPosition - origin);
        }

        public void SetText(string buttonText)
        {
            if (!buttonText.HasContent())
                throw new ArgumentException(buttonText);

            parsedText = buttonText[1..];
            hotKey = (Key)Enum.Parse(typeof(Key), buttonText[0].ToString());

            keyVisual = new(hotKey, TextFont);
            UpdateOptions();
        }

        private void OnKeyPress(Key key)
        {
            if (key == hotKey && Enabled)
            {
                HotKeyPressed?.Invoke();
                keyVisual.OnClick((5 * Scale).Y);
                wasPressed = true;
            }
        }
        private void OnKeyRelease(Key key)
        {
            if (key == hotKey && Enabled && wasPressed)
            {
                HotKeyReleased?.Invoke();
                keyVisual.OnRelease();
                wasPressed = false;
            }
        }

        public void Destroy()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;

            ForceDestroy();
        }
        public void ForceDestroy()
        {
            Input.KeyPressed -= OnKeyPress;
            Input.KeyReleased -= OnKeyRelease;

            Drawer.Unregister(Draw);
        }
    }
}