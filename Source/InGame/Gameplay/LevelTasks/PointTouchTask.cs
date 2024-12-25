using Engine;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using InGame.GameObjects;
using InGame.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InGame.LevelTasks
{
    public class PointTouchTask : ILevelTask
    {
        public CharObject Obj { get; set; } = null;
        
        public int CycleCount { get; private set; }

        private List<Vector2> positions;
        private int touchesPerIteration;
        private int touchCount = 0;
        
        private StepTask moveTask = null;

        public event Action CycleCompleted;

        public PointTouchTask()
        {
            this.touchesPerIteration = LevelConfig.TaskDifficulty;
        }

        public void Start()
        { 
            positions = Level.TopZones.Concat(Level.JumpZones).ToList();

            var font = Fonts.SilkBold;
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
            collider.ColliderEnter += OnTriggerEnter;
            
            Obj.AddModule(collider);
        }
        public void Finish()
        {
            Obj?.Destroy();
            Obj = null;

            moveTask?.Break();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Obj == null)
                return;

            if (other.Owner is not Player)
                return;

            if (moveTask?.IsRunning ?? false)
                return;

            touchCount++;

            Vector2 newPosition;
            moveTask?.Break();
            do
            {
                newPosition = positions.RandomElement();
            }
            while (newPosition == Obj.Position);

            moveTask = StepTask.Run(MoveTo(newPosition));

            if (touchCount >= touchesPerIteration)
            {
                Level.GetObject<Player>().Codes.Push(Code.NewRandom());
                touchCount = 0;
                CycleCount++;
                CycleCompleted?.Invoke();
            }
        }

        private IEnumerator MoveTo(Vector2 end)
        {
            Vector2 start = Obj.Position;
            Vector2 direction = end - start;
            Obj.RotationDeg = ((float)Math.Atan2(direction.Y, direction.X)).Rad2Deg() + 90;

            yield return StepTask.Interpolate((ref float e) =>
            {
                Obj.Position = Vector2.Lerp(Obj.Position, end, e);

                if (Obj.Position.DistanceTo(end) < 1)
                {
                    e = 2;
                    return;
                }

                e += FrameState.DeltaTime;
            });

            Obj.Position = end;
            Obj.RotationDeg = 0;            
        }
    }
}