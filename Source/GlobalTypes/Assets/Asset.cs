using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GlobalTypes
{
    public static class Asset
    {
        public static ContentManager Content { get; set; }

        public static string FontsFolder { get; private set; } = "Fonts/";
        public static string LevelsFolder { get; private set; } = "Levels/";

        public static T Load<T>(string path) => Content.Load<T>(path);
        
        public static SpriteFont LoadFont(string name) => Content.Load<SpriteFont>($"{FontsFolder}{name}");
        public static Texture2D LoadLevelPicture(int index) => Content.Load<Texture2D>($"{LevelsFolder}level_{index}");
    }
}