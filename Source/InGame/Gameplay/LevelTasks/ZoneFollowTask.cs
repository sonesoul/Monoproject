using Engine;
using Engine.Drawing;
using GlobalTypes;
using InGame.GameObjects;
using InGame.Interfaces;
using System;
using System.Collections;
using System.Linq;

namespace InGame.LevelTasks
{
    public class ZoneFollowTask : ILevelTask
    {
        public int CycleCount { get; private set; } = 0;

        public event Action CycleCompleted;

        private StringObject movingObject;
        private Player player = null;
        private StepTask moveTask = null;
        private bool isOnZone = false;

        private float transitionTime = 5;
        private float rewardTime = 2;


        public ZoneFollowTask()
        {
            var font = Fonts.SilkBold;
            string arrow = "->";
            movingObject = new(arrow, font, true)
            {
                IsVisible = false,
                Origin = font.MeasureString(arrow) / 2
            };

            Drawer.Register(Draw);
            
            Action a = SetPlayer;
            a.Wrap(w => Level.Created += w, w => Level.Created -= w);
        }

        private void SetPlayer()
        {
            player = Level.GetObject<Player>();
        }

        private IEnumerator MoveObject() 
        {
            float counter = 0;

            while (true) 
            {
                Vector2 start = movingObject.Position;
                Vector2 end = Level.TopZones.Concat(Level.JumpZones).Where(t => start.DistanceTo(t) > 300 && start.DistanceTo(t) < 500).RandomElement();

                Vector2 dir = end - start;

                movingObject.RotationDeg = ((float)Math.Atan2(dir.Y, dir.X)).Rad2Deg();

                yield return StepTask.Interpolate((ref float e) =>
                {
                    movingObject.Position = Vector2.SmoothStep(start, end, e);

                    float distance = movingObject.Position.DistanceTo(player.Position);
                    isOnZone = movingObject.IsVisible && distance <= 80;

                    e += FrameState.DeltaTime / transitionTime;

                    if (isOnZone)
                    {
                        counter += FrameState.DeltaTime / rewardTime;

                        if (counter >= 1)
                        {
                            counter -= 1;

                            player.Codes.Push(Code.NewRandom());
                            player.Grade.AddPoints(rewardTime / 20);
                            CycleCompleted?.Invoke();
                            CycleCount++;
                        }
                    }
                    
                });
            }           
        }

        private void Draw(DrawContext context)
        {
            context.Circle(movingObject.Position, 50, isOnZone ? Palette.White : new(Palette.White, 0.3f), 2);
        }

        public void Start()
        {
            movingObject.IsVisible = true;

            movingObject.Position = Level.TopZones.RandomElement();

            moveTask = StepTask.Run(MoveObject);
        }

        public void Finish()
        {
            Drawer.Unregister(Draw);

            movingObject?.Destroy();

            moveTask?.Break();
        }
    }
}