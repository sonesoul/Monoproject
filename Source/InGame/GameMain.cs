using Engine;
using Engine.Drawing;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using GlobalTypes.Events;
using InGame.GameObjects;
using Microsoft.Xna.Framework;

namespace InGame
{
    public class GameMain 
    {  
        public GameMain()
        {
            FrameEvents.Update.Add(Update, UpdateOrders.GameMain);
            CreateWalls();
            Level.New();
        }

        private void Update()
        {
            
        }
        private void CreateWalls()
        {
            IngameDrawer ingameDrawer = IngameDrawer.Instance;
            int width = InstanceInfo.WindowWidth;
            int height = InstanceInfo.WindowHeight;
            int offset = 9;

            //bottom
            _ = new StringObject(ingameDrawer, "", UI.Silk, new Collider()
            {
                polygon = Polygon.Rectangle(width, 20),
                Mode = ColliderMode.Static
            }).Position = new(width / 2, height + offset);

            //right
            _ = new StringObject(ingameDrawer, "", UI.Silk, new Collider()
            {
                polygon = Polygon.Rectangle(20, height),
                Mode = ColliderMode.Static
            }).Position = new(width + offset, height / 2);

            //left
            _ = new StringObject(ingameDrawer, "", UI.Silk, new Collider()
            {
                polygon = Polygon.Rectangle(20, height),
                Mode = ColliderMode.Static
            }).Position = new(-offset, height / 2);
        }
    }
}