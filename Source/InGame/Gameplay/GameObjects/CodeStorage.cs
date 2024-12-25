using Engine;
using GlobalTypes;
using InGame.Interfaces;
using InGame.Managers;
using System;
using System.Collections.Generic;

namespace InGame.GameObjects
{
    public class CodeStorage : ModularObject, ILevelObject
    {
        public static event Action<CodeStorage> Created;

        public int Capacity { get; private set; }
        public string Requirement { get; private set; }
        public LevelTimer Timer { get; private set; }

        public int FillReward { get; set; } = 3;

        public float Progress 
        {
            get => fillProgress;
            set
            {
                float prev = fillProgress;
                fillProgress = value.Clamp(0, Capacity);

                if (prev > fillProgress)
                    ProgressUp?.Invoke();
                else if (prev < fillProgress)
                    ProgressDown?.Invoke();

                ProgressChanged?.Invoke();
            }
        }
        public bool IsFilled => Progress >= Capacity;

        public event Action Finished;
        public event Action TimeOver;
        public event Action ProgressUp, ProgressDown, ProgressChanged;

        public event Action<string> RequirementChanged;
        public event Action<Code> Pushed;

        private float fillProgress;
        private readonly List<Code> pushed = new();
        
        public CodeStorage()
        {
            Capacity = LevelConfig.StorageCapacity;

            Timer = new(LevelConfig.TimeSeconds, true);
            Timer.TimeOver += () =>
            {
                TimeOver?.Invoke();
                Level.Fail();
            };

            RollRequirement();

            Created?.Invoke(this);
        }
        public bool Push(Code code)
        {
            string req = Requirement;
            RollRequirement();

            if (!code.Contains(req))
            {
                Progress -= 0.5f;
                FillReward = (FillReward - 1).ClampMin(1);

                return false;
            }

            pushed.Add(code);
            Progress++;

            Pushed?.Invoke(code);

            if (IsFilled)
            {
                Finish();
            }

            return true;
        }
        
        public void Finish()
        {
            Timer.Stop();

            int completeSeconds = (int)LevelConfig.TimeSeconds - Timer.SecondsLeft;

            int additionalReward;
            if (completeSeconds < 20)
            {
                additionalReward = FillReward;
            }
            else if (completeSeconds < 30)
            {
                additionalReward = FillReward / 2;
            }
            else
            {
                additionalReward = 0;
            }

            Level.GetObject<Player>().BitWallet.Deposit(FillReward + additionalReward);

            Finished?.Invoke();

            StepTask.RunDelayed(Level.Complete, () => StepTask.DelayUnscaled(1.5f));
        }
        public void SetRequirement(string c)
        {
            Requirement = c;
            RequirementChanged?.Invoke(Requirement);
        }
        public void RollRequirement()
        {
            SetRequirement(LevelConfig.CodePattern.Chars.RandomElement().ToString());
        } 

        public override void ForceDestroy()
        {
            base.ForceDestroy();

            Timer.Stop();
            Finished = null;
            RequirementChanged = null;
            Pushed = null;
        }

        ~CodeStorage()
        {
            //Monoconsole.WriteLine("Storage dector");
        }
    }
}