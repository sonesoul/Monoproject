using GlobalTypes;
using InGame.Interfaces;
using InGame.Managers;
using InGame.Overlays;
using InGame.Pools;
using System;
using System.Collections;
using System.Collections.Generic;

namespace InGame.Difficulty
{
    public class DifficultyScaler
    {
        public static float BaseFactor { get; set; } = 0f;
        public float ProcProgress { get; private set; } = BaseFactor;
        public float ProcStep { get; set; } = 1f;
        public float IncrementFactor { get; set; } = 1;


        public event Action<IDifficultyModifier> ModifierAdded, ModifierApplied;

        private List<IDifficultyModifier> modifiers = new();
        private Queue<IDifficultyModifier> queuedApplies = new();
        
        private Random random = new();
        private float interval = 10f;
        private StepTask scaleTask = null;
        
        public DifficultyScaler()
        {
            Level.Completed += OnLevelCompleted;
        }

        private void OnLevelCompleted()
        {
            while (queuedApplies.Count > 0)
            {
                var m = queuedApplies.Dequeue();
                m.Apply();
                ModifierApplied?.Invoke(m);
                ProcProgress = BaseFactor;
                IncrementFactor = 1;
            }
        }
        public void AddModifier(IDifficultyModifier modifier)
        {
            modifiers.Add(modifier);
            queuedApplies.Enqueue(modifier);
            ModifierAdded?.Invoke(modifier);
        }

        public void StartScaling()
        {
            StepTask.Replace(ref scaleTask, DifficultyScale);
        }
        public void StopScaling()
        {
            scaleTask?.Break();
        }

        private float CalculateIncrement(float interval)
        {
            float timeFactor = MathF.Log(1 + (Level.TimePlayed / 30));
            
            return (0.03f * timeFactor / (1 / interval)) * IncrementFactor;
        }

        private IEnumerator DifficultyScale()
        {
            while (true)
            {
                PerfomanceOverlay.Info = $"\n{ProcProgress:0.00}";

                float increment = CalculateIncrement(FrameState.DeltaTime);
                ProcProgress += increment;

                if (ProcProgress >= ProcStep)
                {
                    float interval = random.Next(1, (int)this.interval);
                    yield return StepTask.Delay(interval);

                    var modifier = ModifierPool.GetRandomUp();
                    AddModifier(modifier);
                    ProcProgress -= ProcStep;
                    IncrementFactor /= 10;
                }

                yield return null;
            }
        }

        public void CancelAll()
        {
            modifiers.ForEach(m => m.Cancel());
            modifiers.Clear();

            queuedApplies.Clear();
            
            LevelConfig.Reset();
        }
    }
}