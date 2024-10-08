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
            spriteBatch.DrawString(Silk,
                (DrawDebug ? 
                $"{FrameInfo.FPS} / {FrameInfo.DeltaTime}\n" +
                $"{GC.GetTotalMemory(false).ToSizeString()}\n" : 
                "") +
                
                /*$"{playerCombos}\n" +*/
                CustomInfo,
                new Vector2(5, 10), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
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