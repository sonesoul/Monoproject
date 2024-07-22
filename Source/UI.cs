using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static Source.UtilityTypes.HMath;
using Source.UtilityTypes;
using Source.FrameDrawing;
using Source.Engine;
using System.Collections.Generic;

namespace Source.GameUI
{
    public class UI : ILoadable
    {
        public static float fps = 0;
        public static float frameCounter = 0;
        public static double elapsedTime = 0;

        private static SpriteFont font;
        private static InterfaceDrawer interfaceDrawer;
        private static SpriteBatch spriteBatch;

        public void Load()
        {
            GameEvents.Update += GetFps;

            interfaceDrawer = InterfaceDrawer.Instance;
            spriteBatch = interfaceDrawer.SpriteBatch;

            font = GameMain.Instance.Content.Load<SpriteFont>("MainFont");

            interfaceDrawer.AddDrawAction(DrawInfo, DrawMouse);
        }
        public static void GetFps(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (elapsedTime >= 100)
            {
                fps = (int)(frameCounter / (elapsedTime / 1000.0));
                frameCounter = 0;
                elapsedTime = 0;
            }
        }
        public static void DrawInfo(GameTime gameTime)
        {
            frameCounter++;
            Point curPoint = Mouse.GetState().Position;

            Collider coll = GameMain.Instance.player.GetModule<Collider>();

            spriteBatch.DrawString(font,
                $"FPS: {(int)fps}\n" +
                $"Frametime: {HTime.DeltaTime}\n" +
                $"Cursor: [{curPoint.X}:{curPoint.Y}]\n" +
                $"{coll.Info}",
                new Vector2(5, 10), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
        }

        public static void DrawMouse(GameTime gameTime)
        {
            Point curPoint = Mouse.GetState().Position;

            Vector2 curPosition = new(curPoint.X + 6f, curPoint.Y - 6f);

            spriteBatch.DrawString(font, $"<- ", curPosition, Color.White, Deg2Rad(50), new(0, 0), 1.3f, SpriteEffects.None, 0);
            //spriteBatch.DrawString(font, $"[{curPoint.X}:{curPoint.Y}]", new(curPoint.X - 3, curPoint.Y - 40), Color.White, 0, new(0, 0), 1, SpriteEffects.None, 0);
        }
        public static SpriteFont Font => font;
    }
}