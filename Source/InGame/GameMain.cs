using Engine;
using Engine.Drawing;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using GlobalTypes.Events;
using GlobalTypes.InputManagement;

namespace InGame
{
    public class GameMain 
    {
        public GameMain()
        {
            FrameEvents.Update.Add(Update, UpdateOrders.GameMain);
            CreateWalls();
            Level.New();

            Input.Bind(Key.T, KeyPhase.Press, () =>
            {
                new StringObject(IngameDrawer.Instance, "?", UI.Silk)
                {
                    Position = FrameInfo.MousePosition,
                }.AddModule<Rigidbody>();
            });
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
            _ = new StringObject(ingameDrawer, "", UI.Silk, 
                new Collider()
                {
                    Shape = Polygon.Rectangle(width, 20),
                }, 
                new Rigidbody()
                {
                    BodyType = BodyType.Static,
                }).Position = new(width / 2, height + offset);

            //right
            _ = new StringObject(ingameDrawer, "", UI.Silk,
                new Collider()
                {
                    Shape = Polygon.Rectangle(20, height),
                }, 
                new Rigidbody()
                {
                    BodyType = BodyType.Static,
                }).Position = new(width + offset, height / 2);

            //left
            _ = new StringObject(ingameDrawer, "", UI.Silk, 
                new Collider()
                {
                    Shape = Polygon.Rectangle(20, height),
                }, 
                new Rigidbody()
                {
                    BodyType = BodyType.Static,
                }).Position = new(-offset, height / 2);
        }
    }
}