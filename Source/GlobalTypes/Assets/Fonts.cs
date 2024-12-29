using Microsoft.Xna.Framework.Graphics;

namespace GlobalTypes.Assets
{
    public static class Fonts
    {
        public static SpriteFont Silk { get; set; }
        public static SpriteFont SilkBold { get; set; }
        
        public static SpriteFont PicoMono { get; set; }

        static Fonts()
        {
            Silk = Asset.LoadFont("Silkscreen");
            SilkBold = Asset.LoadFont("SilkscreenBold");
            PicoMono = Asset.LoadFont("PicoMono");
        }
    }
}
