using Engine.Drawing;
using GlobalTypes;
using System;

namespace InGame.Overlays
{
    public static class PerfomanceOverlay
    {
        public static string Info { get; set; }

        public static bool IsPerfomanceVisible { get; set; } = true;
        
        private static DrawOptions drawOptions;

        [Init(InitOrders.Perfomance)]
        private static void Init()
        {
            drawOptions = new()
            {
                font = Fonts.SilkBold,
                scale = new Vector2(0.45f, 0.5f),
                position = new(2.5f, 1)
            };

            Drawer.Register(DrawPerfomance, false, 0);
        }

        private static void DrawPerfomance(DrawContext context)
        {
            if (!IsPerfomanceVisible)
                return;

            string spacing = " ".Times(3);

            context.String($"|FPS: {FrameState.FPS}|{spacing}|FTMS: {FrameState.DeltaTimeUnscaled * 1000:00}|{spacing}|MEM: {GC.GetTotalMemory(false).ToSizeString()}|", drawOptions);

            //custom info
            context.String(
                Fonts.Silk,
                Info ?? "",
                new Vector2(5, 10),
                Palette.White,
                Vector2.Zero,
                Vector2.One);
        }
    }
}