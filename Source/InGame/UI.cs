using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Engine.Drawing;
using GlobalTypes;
using System;
using Monoproject;

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


        private static DrawOptions drawOptions;
        
        private static bool IsUsingCustomCursor => !Main.Instance.IsMouseVisible;

        private static void Load()
        {
            Silk = InstanceInfo.Content.Load<SpriteFont>(SilkName);
            SilkBold = InstanceInfo.Content.Load<SpriteFont>(SilkBoldName);
        }
        private static void Init()
        {
            drawOptions = new()
            {
                font = SilkBold,
                scale = new Vector2(0.4f, 0.5f),
                position = new(2, 1)
            };

            Drawer.Register(DrawInfo, matrixDepend: false);
            Drawer.Register(DrawMouse, matrixDepend: false);
        }

        public static void DrawInfo(DrawContext context)
        {
            if (!DrawDebug)
                return;

            context.String($"|fps: {FrameInfo.FPS}  |time: {FrameInfo.DeltaTime * 1000:00}ms  |ram: {GC.GetTotalMemory(false).ToSizeString():00}", drawOptions);

            //custom info
            context.String(
                Silk,
                CustomInfo ?? "",
                new Vector2(5, 10),
                Color.White);
        }
        public static void DrawMouse(DrawContext context)
        {
            if (!IsUsingCustomCursor) 
                return;

            Vector2 curPoint = FrameInfo.MousePosition;
            
            string mouse = "<-";

            Vector2 mouseOrigin = Silk.MeasureString(mouse);
            mouseOrigin.Y /= 2;
            mouseOrigin.X = 0;

            context.String(
                Silk,
                mouse,
                new(curPoint.X - 3, curPoint.Y),
                Color.White,
                45f.AsRad(),
                mouseOrigin, 
                1.3f);
        }
    }
}