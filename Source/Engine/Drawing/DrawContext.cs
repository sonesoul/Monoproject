using InGame.Overlays;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;

namespace Engine.Drawing
{
    public class DrawContext
    {
        public SpriteBatch SpriteBatch { get; private set; }
        public GraphicsDevice Graphics { get; private set; }

        private Texture2D Pixel { get; set; }

        public DrawContext(SpriteBatch batch, GraphicsDevice graphics)
        {
            SpriteBatch = batch;
            Graphics = graphics;

            Pixel = new Texture2D(Graphics, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }

        public void String(SpriteFont font, string str, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;

            if (scale.X < 0)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
                scale.X = -scale.X;
            }

            if (scale.Y < 0)
            {
                if (spriteEffects == SpriteEffects.None)
                {
                    spriteEffects = SpriteEffects.FlipVertically;
                }
                else
                {
                    spriteEffects &= SpriteEffects.FlipVertically;
                }

                scale.Y = -scale.Y;
            }

            SpriteBatch.DrawString(font, str, position, color, rotation, origin, scale, spriteEffects, 0);
        }
        public void String(SpriteFont font, string str, Vector2 position, Color color, Vector2 origin, Vector2 scale)
        {
            String(
                font,
                str,
                position,
                color,
                0,
                origin,
                scale);
        }
        public void String(string str, in DrawOptions options)
        {
            String(
                options.font,
                str,
                options.position,
                options.color,
                options.rotationDeg.Deg2Rad(),
                options.origin,
                options.scale);
        }

        public void Texture(Texture2D texture, in DrawOptions options)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;

            if (options.scale.X < 0)
                spriteEffects = SpriteEffects.FlipHorizontally;

            if (options.scale.Y < 0)
                spriteEffects = SpriteEffects.FlipVertically;

            SpriteBatch.Draw(
                texture,
                options.position,
                null,
                options.color,
                options.rotationDeg.Deg2Rad(),
                options.origin,
                options.scale,
                spriteEffects,
                0);
        }

        public void HollowRect(Rectangle rect, Color color, int boundThickness = 1)
        {
            Rectangle[] rects = new Rectangle[4];

            rects[0] = new(rect.Left, rect.Top, rect.Width, boundThickness);
            rects[1] = new(rect.Left, rect.Top, boundThickness, rect.Height);

            rects[2] = new(rect.Right - boundThickness, rect.Top, boundThickness, rect.Height);
            rects[3] = new(rect.Left, rect.Bottom - boundThickness, rect.Width, boundThickness);

            foreach (var item in rects)
            {
                SpriteBatch.Draw(Pixel, item, color);
            }
        }

        public void Rectangle(Rectangle rect, Color color)
        {
            SpriteBatch.Draw(Pixel, rect, color);
        }

        public void HollowPoly(List<Vector2> vertices, Color color, int boundThickness)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                var current = vertices[i];
                var next = vertices[(i + 1) % vertices.Count];

                Line(current, next, color, boundThickness);
            }
        }
        public void Line(Vector2 start, Vector2 end, Color color, float thickness = 1f)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            Rectangle rect = new((int)start.X, (int)start.Y, (int)edge.Length(), (int)thickness);
            Vector2 origin = new(0, 0.5f);

            SpriteBatch.Draw(Pixel, rect, null, color, angle, origin, SpriteEffects.None, 0);
        }

        public void Circle(Vector2 position, float radius, Color color, float thickness = 1)
        {
            Vector2 point = new(radius);

            DrawOptions options = new()
            {
                color = color,
                scale = new(thickness)
            };

            float i = 0;

            while (i < 1)
            {
                float rotation = MathHelper.LerpPrecise(0, 360, i);

                Vector2 rotated = point.RotateAround(Vector2.Zero, rotation);
                options.position = rotated + (position - (options.scale / 2));

                Texture(Pixel, options);

                i += 1 / (radius * 8);
            }
        }
    }
}