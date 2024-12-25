using GlobalTypes;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Drawing
{
    public static class Drawer
    {
        private class DrawCall
        {
            public DrawAction Action { get; set; }
            public int Layer { get; set; }
            
            public DrawCall(DrawAction action, int layer)
            {
                Action = action;
                Layer = layer;
            }
        }

        public delegate void DrawAction(DrawContext spriteBatch);

        public static int DrawCalls => dynamicActions.Count + staticActions.Count;
        public static Color BackgroundColor => Palette.Black;

        private static SpriteBatch spriteBatch;
        private static GraphicsDevice graphics;

        private readonly static List<DrawCall> dynamicActions = new(), staticActions = new();

        private static DrawContext drawContext;

        [Init(InitOrders.Drawer)]
        private static void Init() 
        {
            spriteBatch = Window.SpriteBatch;
            graphics = Window.GraphicsDevice;

            drawContext = new(spriteBatch, graphics);
        }

        public static void Register(DrawAction action, bool matrixDepend = true, int layer = -1)
        {
            if (dynamicActions.Any(a => a.Action == action) || staticActions.Any(a => a.Action == action))
            {
                throw new InvalidOperationException("Action is already registred.");
            }

            DrawCall task = new(action, layer);

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
            var dynamicFirst = dynamicActions.FirstOrDefault(a => a.Action == action);
            if (dynamicFirst != null)
            {
                dynamicActions.Remove(dynamicFirst);
                return;
            }

            var staticFirst = staticActions.FirstOrDefault(a => a.Action == action);
            if (staticFirst != null)
            {
                staticActions.Remove(staticFirst);
                return;
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
        public static void Erase() => FillWindow(BackgroundColor);
        public static void FillWindow(Color color) => graphics.Clear(color);

        private static void DrawDynamic() => DrawForEach(dynamicActions);
        private static void DrawStatic() => DrawForEach(staticActions);

        private static void DrawForEach(IEnumerable<DrawCall> actions)
        {
            foreach (var item in actions)
            {
                item?.Action?.Invoke(drawContext);
            }
        }

        private static int FirstLarger(DrawCall task, List<DrawCall> tasks)
        {
            int index = tasks.BinarySearch(task, Comparer<DrawCall>.Create((x, y) => x.Layer.CompareTo(y.Layer)));

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
    public struct DrawOptions
    {
        public SpriteFont font = Fonts.Silk;

        public Color color = Palette.White;
        public Vector2 position = Vector2.Zero;
        public Vector2 origin = Vector2.Zero;
        public Vector2 scale = Vector2.One;
        public float rotationDeg = 0;
        
        public DrawOptions()
        {
            
        }
    }   
}