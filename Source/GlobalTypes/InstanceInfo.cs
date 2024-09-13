using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Monoproject;

namespace GlobalTypes
{
    public static class InstanceInfo
    {
        public static GraphicsDevice GraphicsDevice { get; private set; }
        public static GraphicsDeviceManager GraphicsManager { get; private set; }
        public static SpriteBatch SpriteBatch { get; private set; }
        
        public static int WindowWidth => WindowRect.Width;
        public static int WindowHeight => WindowRect.Height;
        public static Rectangle WindowRect { get; set; }
        public static Vector2 WindowSize => new(WindowWidth, WindowHeight);

        public static ContentManager Content { get; private set; }
 
        public static void UpdateVariables()
        {
            Main main = Main.Instance;

            Content = main.Content;
            GraphicsDevice = main.GraphicsDevice;
            GraphicsManager = main.GraphicsManager;
            WindowRect = main.Window.ClientBounds;
            SpriteBatch = main.SpriteBatch;
        }
    }
}