using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Engine.Drawing;
using GlobalTypes.Events;
using GlobalTypes;
using System;
using GlobalTypes.Input;
using GlobalTypes.Attributes;

namespace InGame
{
    [Init(nameof(Init), InitOrders.UI), Load(nameof(Load), LoadOrders.UI)]
    public static class UI
    {
        public static string CustomInfo { get; set; }
        public static float Fps { get; private set; } = 0;
        public static SpriteFont Font { get; private set; }

        private static float frameCounter = 0;
        private static double elapsedTime = 0;

        private static InterfaceDrawer drawer;
        private static SpriteBatch spriteBatch;
        
        private static void Load() => Font = InstanceInfo.Content.Load<SpriteFont>("MainFont");
        private static void Init()
        {
            FrameEvents.Update.Insert(GetFps, EventOrders.Update.UI);

            drawer = InterfaceDrawer.Instance;
            spriteBatch = InstanceInfo.SpriteBatch;

            drawer.AddDrawAction(DrawInfo, DrawMouse);
        }
        public static void GetFps(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (elapsedTime >= 100)
            {
                Fps = (int)(frameCounter / (elapsedTime / 1000.0));
                frameCounter = 0;
                elapsedTime = 0;
            }
        }
        public static void DrawInfo(GameTime gameTime)
        {
            frameCounter++;

            spriteBatch.DrawString(Font,
                $"{(int)Fps} / {FrameState.DeltaTime} \n" +
                $"{GC.GetTotalMemory(false).ToSizeString()}\n" +
                $"{FrameState.MousePosition}\n" +
                CustomInfo,
                new Vector2(5, 10), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
        }
        public static void DrawMouse(GameTime gameTime)
        {
            Vector2 curPoint = FrameState.MousePosition;
            int offset = 6;

            spriteBatch.DrawString(
                Font,
                $"<- ",
                new(curPoint.X + offset, curPoint.Y - offset),
                Color.White,
                50f.AsRad(),
                Vector2.Zero, 
                1.3f, 
                SpriteEffects.None,
                0);
        }
    }
}