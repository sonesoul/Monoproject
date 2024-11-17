using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Engine.Drawing;
using GlobalTypes;
using System;
using Monoproject;

namespace InGame
{
    public static class UI
    {
        public static string CustomInfo { get; set; }
        public static SpriteFont Silk { get; set; }
        public static SpriteFont SilkBold { get; set; }
        public static SpriteFont VCRFont { get; set; }
        
        public static bool IsPerfomanceVisible { get; set; } = true;
        public static bool IsCursorCustom 
        {
            get => !Main.Instance.IsMouseVisible;
            set => Main.Instance.IsMouseVisible = !value;
        }

        private static DrawOptions drawOptions;
        

        [Load(LoadOrders.UI)]
        private static void Load()
        {
            Silk = LoadFont("Silkscreen");
            SilkBold = LoadFont("SilkscreenBold");
            VCRFont = LoadFont("BetterVCR");
        }

        [Init(InitOrders.UI)]
        private static void Init()
        {
            drawOptions = new()
            {
                font = SilkBold,
                scale = new Vector2(0.45f, 0.5f),
                position = new(2.5f, 1)
            };

            Drawer.Register(context =>
            {
                DrawPerfomance(context);
                DrawMouse(context);
            }, 
            matrixDepend: false);
        }

        public static SpriteFont LoadFont(string fontName)
        {
            return MainContext.Content.Load<SpriteFont>($"Fonts/{fontName}");
        }

        private static void DrawPerfomance(DrawContext context)
        {
            if (!IsPerfomanceVisible)
                return;

            context.String($"|FPS: {FrameState.FPS}|   |FTMS: {FrameState.DeltaTime * 1000:00}|   |MEM: {GC.GetTotalMemory(false).ToSizeString()}|", drawOptions);

            //custom info
            context.String(
                Silk,
                CustomInfo ?? "",
                new Vector2(5, 10),
                Color.White);
        }
        private static void DrawMouse(DrawContext context)
        {
            if (!IsCursorCustom) 
                return;

            Vector2 curPoint = FrameState.MousePosition;
            
            string mouse = "<-";

            Vector2 mouseOrigin = Silk.MeasureString(mouse);
            mouseOrigin.Y /= 2;
            mouseOrigin.X = 0;

            context.String(
                Silk,
                mouse,
                new(curPoint.X - 3, curPoint.Y),
                Color.White,
                45f.Deg2Rad(),
                mouseOrigin, 
                1.3f);
        }
    }
}