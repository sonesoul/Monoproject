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
                font = Fonts.PicoMono,
                scale = new Vector2(0.4f),
                position = new(3f, 3f)
            };

            Drawer.Register(DrawPerfomance, false, 0);
        }

        private static void DrawPerfomance(DrawContext context)
        {
            if (!IsPerfomanceVisible)
                return;

            string spacing = " ".Times(3);

            context.String(
                $"{FrameState.FPS}{spacing}({FrameState.DeltaTimeUnscaled * 1000:00}ms){spacing}" +
                $"{GC.GetTotalMemory(false).AsSize().ToLower()}{spacing}" +
                $"{Info}", drawOptions);
        }
    }
}