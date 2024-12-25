using System;

namespace GlobalTypes.Assets
{
    public static class Palette
    {
        public static Color White { get; set; }
        public static Color Black { get; set; }

        private static Color TrueBlack { get; } = Color.Black;
        private static Color TrueWhite { get; } = Color.White;

        private static Color SoftWhite { get; } = new(240, 246, 240);
        private static Color SoftBlack { get; } = new(34, 35, 35);

        public static bool AreColorsSoft { get; private set; } = true;

        static Palette() => SetColors();

        private static void SetColors()
        {
            if (AreColorsSoft)
            {
                White = SoftWhite;
                Black = SoftBlack;
            }
            else
            {
                White = TrueWhite;
                Black = TrueBlack;  
            }
        }
    }
}