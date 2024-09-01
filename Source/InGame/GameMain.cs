using Engine;
using Engine.Drawing;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using GlobalTypes.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monoproject;

namespace InGame
{
    public class GameMain 
    {
        public TextObject player;
        private readonly Main _main;
        private static bool isConsoleTogglePressed = true;
        private TextObject cursorObj;
        
        
        public GameMain(Main instance)
        {
            _main = instance;
            FrameEvents.Update.Insert(Update, EventOrders.Update.GameMain);
            CreateObjects();

            Level.New();
        }

        private void Update(GameTime gameTime)
        {
            HandleInput(FrameState.KeyState, FrameState.MouseState);
        }
        private void HandleInput(KeyboardState keyboard, MouseState mouse)
        {
            if (keyboard.IsKeyDown(Monoconsole.ToggleKey) && !isConsoleTogglePressed)
            {
                Monoconsole.ToggleState();
                isConsoleTogglePressed = true;
            }
            if (keyboard.IsKeyUp(Monoconsole.ToggleKey) && isConsoleTogglePressed)
            {
                isConsoleTogglePressed = false;
            }
            
            cursorObj.position = mouse.Position.ToVector2();

            if (keyboard.IsKeyDown(Keys.F1))
                Monoconsole.Execute("f1");
        }

        private void CreateObjects()
        {
            CreateWalls();

            IngameDrawer ingameDrawer = IngameDrawer.Instance;

            cursorObj = new(ingameDrawer, "", UI.Font, new Collider()
            {
                polygon = Polygon.Rectangle(50, 50),
                Mode = ColliderMode.Static
            });

            _ = new InputZone(ingameDrawer, "Fire", UI.Font)
            {
                position = new(InstanceInfo.WindowWidth / 2, InstanceInfo.WindowHeight - 30)
            };
        }
        private void CreateWalls()
        {
            IngameDrawer ingameDrawer = IngameDrawer.Instance;
            int width = InstanceInfo.WindowWidth;
            int height = InstanceInfo.WindowHeight;
            int offset = 9;

            _ = new TextObject(ingameDrawer, "", UI.Font, new Collider()
            {
                polygon = Polygon.Rectangle(width, 20),
                Mode = ColliderMode.Static
            }).position = new(width / 2, height + offset);
            _ = new TextObject(ingameDrawer, "", UI.Font, new Collider()
            {
                polygon = Polygon.Rectangle(20, height),
                Mode = ColliderMode.Static
            }).position = new(width + offset, height / 2);
            _ = new TextObject(ingameDrawer, "", UI.Font, new Collider()
            {
                polygon = Polygon.Rectangle(20, height),
                Mode = ColliderMode.Static
            }).position = new(-offset, height / 2);
        }
    }
}