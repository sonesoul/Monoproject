using Engine;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using InGame.GameObjects;
using InGame.Generators;
using InGame.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InGame.TaskScripts
{
    public class PointTouchTask
    {
        public CharObject Obj { get; set; }
        public bool IsStarted { get; set; }

        private List<Vector2> positions;
        private int touchesPerCycle;
        private int touchCount = 0;
        
        private StepTask currentTask = null;

        public PointTouchTask(int touchesPerCycle)
        {
            this.touchesPerCycle = touchesPerCycle;
        }

        public void Start()
        {
            if (IsStarted)
                return;

            IsStarted = true;
            
            positions = Level.AbovePlatformTiles.Concat(Level.ReachableTiles).ToList();

            var font = UI.SilkBold;
            Vector2 textSize = font.MeasureString("!");
            
            Obj = new CharObject('!', font, true)
            {
                Position = positions.RandomElement()
            };
            
            Collider collider = new();
            collider.Shape = Polygon.Rectangle(textSize.X, textSize.Y);
            collider.OnOverlapEnter += OnTriggerEnter;
            collider.IsShapeVisible = false;

            Obj.AddModule(collider);
        }
        public void Finish()
        {
            if (!IsStarted)
                return;

            IsStarted = false;

            Obj.Destroy();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.Owner is not Player)
                return;

            touchCount++;

            Vector2 newPosition;
            currentTask?.Break();
            do
            {
                newPosition = positions.RandomElement();
            }
            while (newPosition == Obj.Position);

            currentTask = StepTask.Run(MoveTask(newPosition));

            if (touchCount >= touchesPerCycle)
            {
                Level.GetObject<Player>().PushCombo(Combo.NewRandom());
                touchCount = 0;
            }
        }

        private IEnumerator MoveTask(Vector2 end)
        {
            Vector2 start = Obj.Position;
            float elapsed = 0f;
            Vector2 direction = end - start;
            
            float angle = ((float)Math.Atan2(direction.Y, direction.X)).AsDeg();
            Obj.RotationDeg = angle + 90;
             
            while (elapsed < 1)
            {
                Obj.Position = Vector2.Lerp(Obj.Position, end, elapsed);

                elapsed += FrameInfo.DeltaTime * 2f;
                yield return null;
            }

            Obj.RotationDeg = 0;
            Obj.Position = end;
        }
    }
}
