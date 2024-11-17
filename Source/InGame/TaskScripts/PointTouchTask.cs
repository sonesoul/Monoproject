using Engine;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using InGame.GameObjects;
using InGame.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InGame.TaskScripts
{
    public class PointTouchTask : ILevelTask
    {
        public CharObject Obj { get; set; } = null;
        
        public int CycleCount { get; private set; }

        private List<Vector2> positions;
        private int touchesPerCycle;
        private int touchCount = 0;
        
        private StepTask currentTask = null;

        public event Action OnCycleComplete;

        public PointTouchTask(int touchesPerCycle)
        {
            this.touchesPerCycle = touchesPerCycle;
        }

        public void Start()
        { 
            positions = Level.AbovePlatformTiles.Concat(Level.ReachableTiles).ToList();

            var font = UI.SilkBold;
            Vector2 textSize = font.MeasureString("!");
            
            Obj = new CharObject('!', font, true)
            {
                Position = positions.RandomElement()
            };

            Collider collider = new()
            {
                Shape = Polygon.Rectangle(textSize.X, textSize.Y),
                IsShapeVisible = false
            };
            collider.OnOverlapEnter += OnTriggerEnter;
            
            Obj.AddModule(collider);
        }
        public void Finish()
        {
            Obj?.Destroy();
            Obj = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Obj == null)
                return;

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
                Level.GetObject<Player>().Combo.Push(Combo.NewRandom());
                touchCount = 0;
                CycleCount++;
                OnCycleComplete?.Invoke();
            }
        }

        private IEnumerator MoveTask(Vector2 end)
        {
            Vector2 start = Obj.Position;
            float elapsed = 0f;
            Vector2 direction = end - start;
            
            float angle = ((float)Math.Atan2(direction.Y, direction.X)).Rad2Deg();
            Obj.RotationDeg = angle + 90;
             
            while (elapsed < 1)
            {
                if (Obj == null)
                    yield break;

                Obj.Position = Vector2.Lerp(Obj.Position, end, elapsed);

                elapsed += FrameState.DeltaTime * 2f;
                yield return null;
            }

            Obj.RotationDeg = 0;
            Obj.Position = end;
        }
    }
}