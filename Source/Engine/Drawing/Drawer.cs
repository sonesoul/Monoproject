using GlobalTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Drawing
{
    [Init(nameof(Init), InitOrders.FrameDrawing)]
    public static class Drawer
    {
        private class DrawTask
        {
            public DrawAction Action { get; set; }
            public int Layer { get; set; }
            
            public DrawTask(DrawAction action, int layer)
            {
                Action = action;
                Layer = layer;
            }
        }

        private static SpriteBatch spriteBatch;
        private static GraphicsDevice graphics;

        private static List<DrawTask> dynamicActions = new(), staticActions = new();

        private static DrawContext drawContext;
        public static Color BackgroundColor { get; set; } = Color.Black;

        public delegate void DrawAction(DrawContext spriteBatch);

        private static void Init() 
        {
            spriteBatch = InstanceInfo.SpriteBatch;
            graphics = InstanceInfo.GraphicsDevice;

            drawContext = new(spriteBatch, graphics);
        }

        public static void Register(DrawAction action, bool matrixDepend = true, int layer = -1)
        {
            if (dynamicActions.Any(a => a.Action == action) || staticActions.Any(a => a.Action == action))
            {
                throw new InvalidOperationException("Action is already registred.");
            }

            if (layer < 0)
                layer = 0;

            DrawTask task = new(action, layer);

            if (matrixDepend) 
            {
                int index = FirstLarger(task, dynamicActions);

                dynamicActions.Insert(index, task);
            }
            else
            {
                int index = FirstLarger(task, staticActions);

                staticActions.Insert(index, task);
            }
        }
        public static void Unregister(DrawAction action) 
        {
            dynamicActions.RemoveAll(a => a.Action == action);
            staticActions.RemoveAll(a => a.Action == action);
        }

        private static void DrawDynamic() => DrawForEach(dynamicActions);
        private static void DrawStatic() => DrawForEach(staticActions);

        private static void DrawForEach(IEnumerable<DrawTask> actions)
        {
            foreach (var action in actions)
            {
                action?.Action?.Invoke(drawContext);
            }
        }

        public static void DrawAll()
        {
            SpriteBatch batch = spriteBatch;
            
            batch.Begin(blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp, transformMatrix: Camera.GetViewMatrix());
            DrawDynamic();
            batch.End();

            batch.Begin(blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp);
            DrawStatic();
            batch.End();
        }
        public static void Erase() => graphics.Clear(BackgroundColor);

        private static int FirstLarger(DrawTask task, List<DrawTask> tasks)
        {
            int index = tasks.BinarySearch(task, Comparer<DrawTask>.Create((x, y) => x.Layer.CompareTo(y.Layer)));

            if (index >= 0)
            {
                while (index < tasks.Count && tasks[index].Layer == task.Layer)
                {
                    index++;
                }
                return index;
            }
            else
            {
                return ~index;
            }
        }
    }
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

        public void String(SpriteFont font, string str, Vector2 position, Color color, float rotation = 0, Vector2 origin = default, float scale = 1)
        {
            SpriteBatch.DrawString(font, str, position, color, rotation, origin, scale, SpriteEffects.None, 0);
        }

        public void String(SpriteFont font, string str, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            SpriteBatch.DrawString(font, str, position, color, rotation, origin, scale, spriteEffects, 0);
        }


        public void Char(SpriteFont font, char character, Vector2 position, Color color, float rotation = 0, Vector2 origin = default, float scale = 1)
        {
            String(font, character.ToString(), position, color, rotation, origin, scale);
        }
        public void Char(SpriteFont font, char character, Vector2 position, Color color, float rotation = 0, Vector2 origin = default, Vector2 scale = default, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            String(font, character.ToString(), position, color, rotation, origin, scale, spriteEffects);
        }

        public void HollowRect(Rectangle rect, Color color, int boundThickness = 1)
        {
            Rectangle[] rects = new Rectangle[4];

            rects[0] = new(rect.Left, rect.Top, rect.Width, boundThickness);
            rects[1] = new(rect.Left, rect.Bottom, rect.Width, boundThickness);
            rects[2] = new(rect.Left, rect.Top, boundThickness, rect.Height);
            rects[3] = new(rect.Right, rect.Top, boundThickness, rect.Height);
            
            foreach (var item in rects)
            {
                SpriteBatch.Draw(Pixel, item, color);
            }
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
    }

    public static class Camera
    {
        public static float Zoom
        {
            get => _zoom;
            set => _zoom = Math.Max(0, value);
        }
        public static Vector2 Position
        {
            get => _position;
            set => _position = value;
        }
     
        private static float _zoom = 1f;
        private static Vector2 _position = Vector2.Zero;

        public static Vector2 ScreenToWorld(Vector2 screenPosition) => Vector2.Transform(screenPosition, Matrix.Invert(GetViewMatrix()));
        public static Vector2 WorldToScreen(Vector2 worldPositon) => Vector2.Transform(worldPositon, GetViewMatrix());

        public static Matrix GetViewMatrix() => Matrix.CreateTranslation(new(-_position, 0)) * Matrix.CreateScale(_zoom, _zoom, 1);
    }
}