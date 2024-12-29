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
        public LevelTimer Timer { get; private set; }
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
        public event Action TimeOver;
        
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
            
            Timer = new(LevelConfig.TimeSeconds, true);
            Timer.TimeOver += () =>
            {
                TimeOver?.Invoke();
                Level.Fail();
            };

            RollRequirement();

            Action setFiller = () => filler = Level.GetObject<StorageFiller>();
            setFiller.Invoke(w => Level.Created += w, w => Level.Created -= w);

            Created?.Invoke(this);
        }
        
        public void Push(Code code)
        {
            pushedCodes.Add(new(Requirement, code));
            Progress += code.Length;

            Pushed?.Invoke(code);

            if (IsFilled)
            {
                Filled?.Invoke();
                Finish();
            }
            else
            {
                RollRequirement();
            }
        }
        public void Finish()
        {
            Timer.Stop();
            float baseReward = 7;

            CompletionGrade.Value = CalculateGradeValue();
            Level.GetObject<Player>()?.BitWallet.Deposit((int)(baseReward * CompletionGrade.Value));

            StepTask.RunDelayed(Level.Complete, () => StepTask.DelayUnscaled(3f));
        }

        public void SetRequirement(string c)
        {
            Requirement = c;
            RequirementChanged?.Invoke(Requirement);
        }
        public void RollRequirement() => SetRequirement(LevelConfig.CodePattern.CharSet.RandomElement().ToString());

        private float CalculateGradeValue()
        {
            CodePattern pattern = LevelConfig.CodePattern;
           
            int lastUnmatchIndex = pushedCodes.Select(c => c.MatchesRequirement).ToList().LastIndexOf(false);
            int matchedPushes = pushedCodes.Count - lastUnmatchIndex - 1;
            int maxMatches = Math.Max(Capacity, pattern.Length) / pattern.Length;

            float averageInputTime = filler.TotalInputTime / pushedCodes.Count;

            float matchFactor = (float)maxMatches / 10 * ((float)pattern.CharCount / pattern.Length) * (matchedPushes / maxMatches);
            float mistakeFactor = -(0.05f) * MathF.Pow(filler.MistakeCount, 1.3f);
            float inputTimeFactor = 0.3f / averageInputTime;
            float inputDifficultyFactor = 0.08f / averageInputTime * pattern.Length;

            float total = mistakeFactor + matchFactor + inputDifficultyFactor + inputTimeFactor;

            return total;
        }

        public string GetGrade() => CompletionGrade.ToString();

        public override void ForceDestroy()
        {
            base.ForceDestroy();

            Timer.Stop();
            Filled = null;
            RequirementChanged = null;
            Pushed = null;
        }

        //~CodeStorage() => Monoconsole.WriteLine("Storage dector");
    }
}