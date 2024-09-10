using Engine;
using Engine.Drawing;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using GlobalTypes.Events;
using GlobalTypes.Input;
using Microsoft.Xna.Framework;

namespace InGame
{
    public class GameMain 
    {
        public TextObject player;
        private TextObject cursorObj;
        
        public GameMain()
        {
            FrameEvents.Update.Add(Update, UpdateOrders.GameMain);
            CreateObjects();

            Level.New();
        }

        private void Update(GameTime gameTime)
        {
            cursorObj.position = FrameState.MousePosition;
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