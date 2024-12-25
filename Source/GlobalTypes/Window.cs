using Microsoft.Xna.Framework.Graphics;
using Monoproject;
using System;

namespace GlobalTypes
{
    public static class Window
    {
        public static GraphicsDevice GraphicsDevice { get; private set; }
        public static GraphicsDeviceManager GraphicsManager { get; private set; }
        public static SpriteBatch SpriteBatch { get; private set; }
        
        public static int Width => Rect.Width;
        public static int Height => Rect.Height;
        public static Rectangle Rect { get; set; }
        public static Vector2 Center => Size / 2;
        public static Vector2 Size => new(Width, Height);


        public static event Action Focused; 
        public static event Action Unfocused;

        public static void UpdateInfo()
        {
            Main main = Main.Instance;
            
            GraphicsDevice = main.GraphicsDevice;
            GraphicsManager = main.GraphicsManager;
            Rect = main.Window.ClientBounds;
            SpriteBatch = main.SpriteBatch;

            main.Activated += (args, e) => Focused?.Invoke();
            main.Deactivated += (args, e) => Unfocused?.Invoke();
        }
    }
}