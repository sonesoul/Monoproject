using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Engine.Drawing;
using GlobalTypes;
using System;
using Monoproject;
using InGame.GameObjects;
using System.Linq;

namespace InGame
{
    [Init(nameof(Init), InitOrders.UI), Load(nameof(Load), LoadOrders.UI)]
    public static class UI
    {
        public static string CustomInfo { get; set; }
        public static SpriteFont Silk { get; set; }
        public static SpriteFont SilkBold { get; set; }

        public static string SilkName => "Silkscreen";
        public static string SilkBoldName => "SilkscreenBold";

        public static bool DrawDebug { get; set; } = true;
        public static bool UseCustomCursor 
        {
            get => IsUsingCustomCursor;
            set => Main.Instance.IsMouseVisible = !value;
        }
        private static bool IsUsingCustomCursor => !Main.Instance.IsMouseVisible;
       
        private static InterfaceDrawer drawer;
        private static SpriteBatch spriteBatch;
        
        private static void Load()
        {
            Silk = InstanceInfo.Content.Load<SpriteFont>(SilkName);
            SilkBold = InstanceInfo.Content.Load<SpriteFont>(SilkBoldName);
        }
        private static void Init()
        {
            drawer = InterfaceDrawer.Instance;
            spriteBatch = InstanceInfo.SpriteBatch;

            drawer.AddDrawAction(DrawInfo, DrawMouse);
        }

        public static void DrawInfo()
        {
            Vector2 fpsPos = new(1, 1);
            float size = 0.7f;

            spriteBatch.DrawString(
                Silk,
                $"|fps: {FrameInfo.FPS}",
                fpsPos, 
                Color.White,
                0, 
                Vector2.Zero,
                size, 
                SpriteEffects.None, 
                0);

            spriteBatch.DrawString(
                Silk,
                $"|ft: {FrameInfo.DeltaTime * 1000:00}ms",
                fpsPos.WhereX(x => x + 70),
                Color.White,
                0,
                Vector2.Zero,
                size,
                SpriteEffects.None,
                0);

            spriteBatch.DrawString(
               Silk,
               $"|ram: {GC.GetTotalMemory(false).ToSizeString():00}",
               fpsPos.WhereX(x => x + 150),
               Color.White,
               0,
               Vector2.Zero,
               size,
               SpriteEffects.None,
               0);

            spriteBatch.DrawString(
                Silk,
                CustomInfo ?? "",
                new Vector2(5, 10),
                Color.White, 
                0, 
                new Vector2(0, 0),
                1f, 
                SpriteEffects.None,
                0);
        }
        public static void DrawMouse()
        {
            if (!IsUsingCustomCursor) 
                return;

            Vector2 curPoint = FrameInfo.MousePosition;
            
            string mouse = "<-";

            Vector2 mouseOrigin = Silk.MeasureString(mouse);
            mouseOrigin.Y /= 2;
            mouseOrigin.X = 0;
            spriteBatch.DrawString(
                Silk,
                mouse,
                new(curPoint.X - 3, curPoint.Y),
                Color.White,
                45f.AsRad(),
                mouseOrigin, 
                1.3f, 
                SpriteEffects.None,
                0);
        }
    }
}