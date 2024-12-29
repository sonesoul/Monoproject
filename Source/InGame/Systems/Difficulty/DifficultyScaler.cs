using GlobalTypes;
using GlobalTypes.Interfaces;
using InGame.Interfaces;
using InGame.Overlays;
using InGame.Pools;
using System;
using System.Collections;
using System.Collections.Generic;

namespace InGame.Difficulty
{
    public class DifficultyScaler : IDestroyable
    {
        public float ProcProgress { get; private set; } = 0f;
        public float IncrementFactor { get; private set; } = 1;
        public bool IsDestroyed { get; set; }

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
                IncrementFactor = 1;
            }
        }
        public void AddModifier(IDifficultyModifier modifier)
        {
            modifiers.Add(modifier);
            queuedApplies.Enqueue(modifier);
            ModifierAdded?.Invoke(modifier);
        }

        public void StartScaling() => StepTask.Replace(ref scaleTask, DifficultyScale);
        public void StopScaling() => scaleTask?.Break();

        private float CalculateIncrement(float interval)
        {
            float timeFactor = MathF.Log(1 + ((Level.TimePlayed / 5).Clamp01()));
            
            return (0.04f * timeFactor / (1 / interval)) * IncrementFactor;
        }

        private IEnumerator DifficultyScale()
        {
            while (true)
            {
                float increment = CalculateIncrement(FrameState.DeltaTime);
                ProcProgress += increment;

                if (ProcProgress >= 1)
                {
                    var modifier = ModifierPool.GetRandomUp();
                    AddModifier(modifier);
                    ProcProgress--;
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

            ResetProgress();

            LevelConfig.Reset();
        }

        public void ResetProgress()
        {
            ProcProgress = 0;
            IncrementFactor = 1;
        }

        public void Destroy() => IDestroyable.Destroy(this);
        public void ForceDestroy()
        {
            Level.Completed -= OnLevelCompleted;
        }
    }
}