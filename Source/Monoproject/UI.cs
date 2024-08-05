using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Engine.Drawing;
using Engine.Modules;
using GlobalTypes.Events;
using GlobalTypes;
using GlobalTypes.Interfaces;

namespace Monoproject.GameUI
{
    public class UI : ILoadable
    {
        public static float Fps { get; private set; } = 0;
        private static float frameCounter = 0;
        private static double elapsedTime = 0;

        private static SpriteFont font;
        private static InterfaceDrawer interfaceDrawer;
        private static SpriteBatch spriteBatch;
        
        public void Load()
        {
            GameEvents.OnUpdate.AddListener(GetFps);

            interfaceDrawer = InterfaceDrawer.Instance;
            spriteBatch = interfaceDrawer.SpriteBatch;

            font = Main.Instance.Content.Load<SpriteFont>("MainFont");

            interfaceDrawer.AddDrawAction(DrawInfo, DrawMouse);
        }
        public static void GetFps(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (elapsedTime >= 100)
            {
                Fps = (int)(frameCounter / (elapsedTime / 1000.0));
                frameCounter = 0;
                elapsedTime = 0;
            }
        }
        public static void DrawInfo(GameTime gameTime)
        {
            frameCounter++;
            Point curPoint = Mouse.GetState().Position;

            Collider coll = Main.Instance?.player?.GetModule<Collider>();

            spriteBatch.DrawString(font,
                $"FPS: {(int)Fps}\n" +
                $"Frametime: {HTime.DeltaTime}\n" +
                $"Cursor: [{curPoint.X}:{curPoint.Y}]\n" +
                $"{coll?.info}",
                new Vector2(5, 10), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
        }

        public static void DrawMouse(GameTime gameTime)
        {
            Point curPoint = Mouse.GetState().Position;
              
            Vector2 curPosition = new(curPoint.X + 6f, curPoint.Y - 6f);

            spriteBatch.DrawString(font, $"<- ", curPosition, Color.White, 50f.AsRad(), new(0, 0), 1.3f, SpriteEffects.None, 0);
        }
        public static SpriteFont Font => font;
    }
}