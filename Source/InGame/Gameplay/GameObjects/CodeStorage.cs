using Engine;
using GlobalTypes;
using InGame.Interfaces;
using InGame.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InGame.GameObjects
{
    public class CodeStorage : ModularObject, ILevelObject
    {
        private readonly struct PushInfo
        {
            public bool MatchesRequirement { get; }
            public Code Code { get; }

            public PushInfo(string requirement, Code code)
            {
                Code = code;
                MatchesRequirement = code.Contains(requirement);
            }
        }

        public static event Action<CodeStorage> Created;

        public int Capacity { get; private set; }
        public string Requirement { get; private set; }
        public Grade CompletionGrade { get; private set; } = new();

        public int TotalPushes => pushedCodes.Count;
        public int MatchedPushes => pushedCodes.Where(c => c.MatchesRequirement).Count();

        public float Progress 
        {
            get => _progress;
            set
            {
                float prev = _progress;
                _progress = value.Clamp(0, Capacity);

                ProgressChanged?.Invoke(_progress - prev);
            }
        }
        public bool IsFilled => Progress >= Capacity;

        public event Action Filled;
        
        public event Action<float> ProgressChanged;
        public event Action<string> RequirementChanged;
        public event Action<Code> Pushed;

        private float _progress;
        private List<PushInfo> pushedCodes = new();
        private StorageFiller filler;

        public CodeStorage()
        {
            var codeLength = LevelConfig.CodeLength;
            float charsCount = LevelConfig.CodePattern.CharSet.Length;
            
            float rewardFactor = (charsCount * 2 / codeLength).ClampMin(1);

            Capacity = LevelConfig.StorageCapacity;
            
            RollRequirement();

            Action setFiller = () => filler = Level.GetObject<StorageFiller>();
            setFiller.Wrap(w => Level.Created += w, w => Level.Created -= w);

            Created?.Invoke(this);
        }
        
        public void Push(Code code)
        {
            if (IsFilled)
                return;

            pushedCodes.Add(new(Requirement, code));
            Progress += code.Length;

            Pushed?.Invoke(code);

            var player = Level.GetObject<Player>();
            if (player != null) 
            {
                float pushFactor = CalculatePushFactor();
                player?.Grade.AddPoints(pushFactor);
                player?.BitWallet.Deposit((int)(10f * pushFactor));
            }

            if (IsFilled)
            {
                StepTask.RunDelayed(Level.Complete, () => StepTask.DelayUnscaled(1f));
                Filled?.Invoke();
            }
            else
            {
                RollRequirement();
            }
        }

        public void SetRequirement(string c)
        {
            Requirement = c;
            RequirementChanged?.Invoke(Requirement);
        }
        public void RollRequirement() => SetRequirement(LevelConfig.CodePattern.CharSet.RandomElement().ToString());

        private float CalculatePushFactor()
        {
            CodePattern pattern = LevelConfig.CodePattern;

            float inputDifficultyFactor = 0.2f * (pattern.Length / 5f / filler.LastInputTime); 
            float matchFactor = 0.2f * ((float)pattern.CharCount / pattern.Length);

            return Math.Max(matchFactor, inputDifficultyFactor);
        }
        
        public override void ForceDestroy()
        {
            base.ForceDestroy();

            Filled = null;
            RequirementChanged = null;
            Pushed = null;
        }

        //~CodeStorage() => Monoconsole.WriteLine("Storage dector");
    }
}